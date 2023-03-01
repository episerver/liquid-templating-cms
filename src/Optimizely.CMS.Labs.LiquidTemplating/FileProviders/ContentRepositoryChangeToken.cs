using EPiServer;
using EPiServer.Core;
using EPiServer.Events.Clients;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Primitives;
using Optimizely.CMS.Labs.LiquidTemplating.Content;
using System;
using System.Diagnostics;
using System.Linq;

namespace Optimizely.CMS.Labs.LiquidTemplating.FileProviders
{
    /// <summary>
    /// Implementation of IChangeToken encapsulating method of determining whether LiquidTemplateData Content has changed
    /// </summary>
    public class ContentRepositoryChangeToken : IChangeToken, IDisposable
    {
        private IContentEvents _events;
        private Event _liquidEvent;
        private IContentLoader _contentLoader;

        public ContentRepositoryChangeToken()
        {
            _contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            _events = ServiceLocator.Current.GetInstance<IContentEvents>();
            _events.PublishedContent += Events_PublishedContent;
            _events.MovedContent += Events_MovedContent;

            var eventsRegistry = ServiceLocator.Current.GetInstance<IEventRegistry>();
            _liquidEvent = eventsRegistry.Get(new Guid(Constants.EventGuid));
            _liquidEvent.Raised += LiquidEvent_Raised;
        }

        public bool ActiveChangeCallbacks => true;

        public bool HasChanged { get; internal set; }

        private object _state;
        private Action<object> _callback;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            _state = state;
            _callback = callback;

            return this;
        }

        public void Dispose()
        {
            _events.PublishedContent -= Events_PublishedContent;
            _liquidEvent.Raised -= LiquidEvent_Raised;
            
            _state = null;
            _callback = null;
        }

        private void Events_PublishedContent(object sender, ContentEventArgs e)
        {
            var liquidRoot = _contentLoader.GetAncestors(e.ContentLink).Where(c => c.ContentGuid.ToString() == Constants.RootGuid);
            
            if (e.Content is LiquidTemplateData || liquidRoot != null)
            {
                //Raise CMS event to ensure change is processed on all servers
                _liquidEvent.Raise(new Guid(Constants.RaiserGuid), e.Content.ContentLink.ID);
            }
        }

        private void Events_MovedContent(object sender, ContentEventArgs e)
        {
            var liquidRoot = _contentLoader.GetAncestors(e.ContentLink).Where(c => c.ContentGuid.ToString() == Constants.RootGuid);

            if (e.Content is LiquidTemplateData || liquidRoot != null)
            {
                //Raise CMS event to ensure change is processed on all servers
                _liquidEvent.Raise(new Guid(Constants.RaiserGuid), e.Content.ContentLink.ID);
            }
        }

        private void LiquidEvent_Raised(object sender, EPiServer.Events.EventNotificationEventArgs e)
        {
            Debug.WriteLine("LiquidEvent_Raised");
            //Invoke registered callbacks to notify FileProvider / consumers of change
            HasChanged = true;
            _callback.Invoke(_state);
        }
    }
}
