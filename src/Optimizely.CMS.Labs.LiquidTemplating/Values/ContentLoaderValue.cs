using EPiServer;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Fluid;
using Fluid.Values;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Optimizely.CMS.Labs.LiquidTemplating.Values
{
    public class ContentLoaderValue : ObjectValueBase
    {
        private static IContentLoader contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

        public static Dictionary<string, VirtualMemberDelegate> methodMap = new Dictionary<string, VirtualMemberDelegate>();
        public delegate FluidValue VirtualMemberDelegate(FunctionArguments arguments, TemplateContext context);

        public ContentLoaderValue() : base(new object())
        {
            //ContentLoader methods
            AddMethod(Get);
            AddMethod(GetChildren);
            AddMethod(GetAncestors);
            AddMethod(GetDescendants);
            //Custom helpers
            AddMethod(GetStartPage);
            AddMethod(GetCrumbtrail);
            AddMethod(GetDepth);
            AddMethod(GetNearestOfType);
            AddMethod(GetNearestWithValue);
            AddMethod(GetBranch);
            AddMethod(GetParent);
            AddMethod(GetSiblings);

            void AddMethod(VirtualMemberDelegate m)
            {
                methodMap[m.Method.Name.ToLower()] = m;
            }
        }

        public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
        {
            // First, see if we have a mapped method
            var key = name.ToLower();
            if (methodMap.ContainsKey(key))
            {
                return new FunctionValue(new Func<FunctionArguments, TemplateContext, FluidValue>(methodMap[key]));
            }

            // Do other GetValue stuff here...
            return NilValue.Instance;
        }

        // {{ ContentLoader.Get(Model.CurrentPage.ContentLink).Name }}
        // {{ ContentLoader.Get(Model.CurrentPage.ContentLink, "fr").Name }}
        // 1. ContentReference
        // 2. Language string
        public static FluidValue Get(FunctionArguments a, TemplateContext c)
        {
            var contentref = a.At(0).ToObjectValue() as ContentReference;
            var languageName = a.At(1).Or(new StringValue(ContentLanguage.PreferredCulture.Name)).ToStringValue();
            var language = new CultureInfo(languageName);

            var result = contentLoader.Get<ContentData>(contentref, language);

            return new ObjectValue(result);
        }

        // {% for page in ContentLoader.GetChildren(Model.CurrentPage) %}
        // {% for page in ContentLoader.GetChildren(Model.CurrentPage, "en") %}
        // 1. ContentReference
        // 2. Language string
        public static FluidValue GetChildren(FunctionArguments a, TemplateContext c)
        {
            var contentref = a.At(0).ToObjectValue() as ContentReference;
            var languageName = a.At(1).Or(new StringValue(ContentLanguage.PreferredCulture.Name)).ToStringValue();
            var language = new CultureInfo(languageName);

            var results = contentLoader.GetChildren<ContentData>(contentref, language);
            return new ArrayValue(results.Select(r => new ObjectValue(r)));
        }

        // {% for page in ContentLoader.GetAncestors(Model.CurrentPage.ContentLink) %}
        // 1. ContentReference
        // 2. Boolean reverse (optional, default:false) 
        public static FluidValue GetAncestors(FunctionArguments a, TemplateContext c)
        {
            var contentref = a.At(0).ToObjectValue() as ContentReference;
            var reverse = a.At(1).Or(BooleanValue.Create(false)).ToBooleanValue();

            var results = GetAncestors(contentref);

            if (reverse)
            {
                results = results.Reverse();
            }

            return new ArrayValue(results);
        }

        // {% for page in ContentLoader.GetDescendants(Model.CurrentPage.ContentLink) %}
        // 1. ContentReference
        public static FluidValue GetDescendants(FunctionArguments a, TemplateContext c)
        {
            var contentref = a.At(0).ToObjectValue() as ContentReference;
            var results = contentLoader.GetDescendents(contentref).Select(r => contentLoader.Get<ContentData>(r));
            return new ArrayValue(results.Select(r => new ObjectValue(r)));
        }

        // {{ ContentLoader.GetStartPage().Name }}
        // {{ ContentLoader.GetStartPage("fr").Name }}
        // 1. Language string
        public FluidValue GetStartPage(FunctionArguments a, TemplateContext c)
        {
            var languageName = a.At(0).Or(new StringValue(ContentLanguage.PreferredCulture.Name)).ToStringValue();
            var language = new CultureInfo(languageName);
            return new ObjectValue(contentLoader.Get<PageData>(ContentReference.StartPage, language));
        }


        // {{ ContentLoader.GetParent(Model.CurrentPage).Name }}
        // {{ ContentLoader.GetParent(Model.CurrentPage.ContentLink).Name }}
        // {{ ContentLoader.GetParent(26).Name }}
        // 1. ContentReference
        public static FluidValue GetParent(FunctionArguments a, TemplateContext c)
        {
            var contentref = GetContentReference(a.At(0).ToObjectValue());

            var results = contentLoader.Get<ContentData>(contentLoader.Get<IContent>(contentref).ParentLink);
            return new ObjectValue(results);
        }

        // {% for page in ContentLoader.TopLevelPages() %}
        public static FluidValue GetTopLevelPages(FunctionArguments a, TemplateContext c)
        {
            return new ArrayValue(GetChildren(ContentReference.StartPage));
        }

        // {% for page in ContentLoader.GetNearestOfType(Model.CurrentPage, "ProductPage") %}
        // {% for page in ContentLoader.GetNearestOfType(Model.CurrentPage.ContentLink, "ProductPage") %}
        // {% for page in ContentLoader.GetNearestOfType(23, "ProductPage") %}
        // 1. ContentReference
        // 2. String typeName  
        public static FluidValue GetNearestOfType(FunctionArguments a, TemplateContext c)
        {
            var contentref = GetContentReference(a.At(0).ToObjectValue());
            var typeName = a.At(1).ToStringValue();

            var ancestors = contentLoader.GetAncestors(contentref);
            var result = ancestors.Select(r => contentLoader.Get<PageData>(r.ContentLink)).FirstOrDefault(p => p.PageTypeName == typeName);

            return result != null ? new ObjectValue(result) : NilValue.Instance;
        }

        // {% for page in ContentLoader.GetNearestWithValue(Model.CurrentPage, "MetaDescription") %}
        // {% for page in ContentLoader.GetNearestWithValue(Model.CurrentPage.ContentLink, "MetaDescription") %}
        // {% for page in ContentLoader.GetNearestWithValue(54, "MetaDescription") %}
        // 1. ContentReference
        // 2. String PropertyName   
        public static FluidValue GetNearestWithValue(FunctionArguments a, TemplateContext c)
        {
            var contentref = a.At(0).ToObjectValue() as ContentReference;
            var propertyName = a.At(1).ToStringValue();

            var ancestors = contentLoader.GetAncestors(contentref);
            var result = ancestors.Select(r => contentLoader.Get<ContentData>(r.ContentLink)).FirstOrDefault(p => p.Property[propertyName] != null && p.Property[propertyName].Value != null);

            return result != null ? new ObjectValue(result) : NilValue.Instance;
        }

        // {% if ContentLoader.GetDepth(Model.CurrentPage) > 1 %}
        // {% if ContentLoader.GetDepth(Model.CurrentPage.ContentLink) > 1 %}
        // {% if ContentLoader.GetDepth(6) > 1 %}
        public static FluidValue GetDepth(FunctionArguments a, TemplateContext c)
        {
            var contentref = GetContentReference(a.At(0).ToObjectValue());

            var results = contentLoader.GetAncestors(contentref);
            return NumberValue.Create(results.Count());
        }

        // {% for page in ContentLoader.GetSiblings(Model.CurrentPage, true) %}
        // {% for page in ContentLoader.GetSiblings(Model.CurrentPage.ContentLink, true) %}
        // {% for page in ContentLoader.GetSiblings(67, true) %}
        // 1. Boolean; include target page (optional, default:true) 
        public static FluidValue GetSiblings(FunctionArguments a, TemplateContext c)
        {
            var contentref = GetContentReference(a.At(0).ToObjectValue());
            var includeTarget = a.At(1).Or(BooleanValue.Create(true)).ToBooleanValue();

            var results = GetChildren(contentLoader.Get<IContent>(contentref).ParentLink);

            if (!includeTarget)
            {
                results = results.Where(o => GetContentReference(o.ToObjectValue()) != contentref);
            }

            return new ArrayValue(results);
        }

        // {% for page in ContentLoader.GetCrumbtrail(Model.CurrentPage) %}
        // {% for page in ContentLoader.GetCrumbtrail(Model.CurrentPage.ContentLink) %}
        // {% for page in ContentLoader.GetCrumbtrail(82) %}
        // 1. PageData or ContentReference
        // 2. Boolean; include home page (optional, default:true) 
        // 3. Boolean; include target page (optional, default:true) 
        public static FluidValue GetCrumbtrail(FunctionArguments a, TemplateContext c)
        {
            var contentref = GetContentReference(a.At(0).ToObjectValue());
            var includeHome = a.At(1).Or(BooleanValue.Create(true)).ToBooleanValue();
            var includeCurrent = a.At(2).Or(BooleanValue.Create(true)).ToBooleanValue();

            var results = GetAncestors(contentref);

            if (results.Count() == 1) // This would only be if they called this on the StartPage itself
            {
                return new ArrayValue(new List<ObjectValue>()); // Send back an empty list
            }

            results = results.Take(results.Count() - 1); // Throw away the last one, which will be root

            if (!includeHome)
            {
                results = results.Take(results.Count() - 1); // Throw away the last one, which will be home
            }

            results = results.Reverse();

            if (includeCurrent)
            {
                var current = contentLoader.Get<PageData>(contentref);
                results = results.Concat(new[] { new ObjectValue(current) });
            }

            return new ArrayValue(results);
        }

        // {% for page in ContentLoader.GetBranch(Model.CurrentPage) %}
        // {% for page in ContentLoader.GetBranch(Model.CurrentPage.ContentLink) %}
        // {% for page in ContentLoader.GetBranch(82) %}
        // 1. PageData or ContentReference
        // 2. int; max depth (optional, default int.max) 
        // 3. Boolean; include rootNode (optional, default:true) 
        public static FluidValue GetBranch(FunctionArguments a, TemplateContext c)
        {
            var contentref = GetContentReference(a.At(0).ToObjectValue());
            var maxDepth = a.At(1).Or(NumberValue.Create(int.MaxValue)).ToNumberValue();
            var includeRootNode = a.At(2).Or(BooleanValue.Create(true)).ToBooleanValue();

            var rootNode = new ValueNode<PageData>(contentLoader.Get<PageData>(contentref));
            var depth = 1;
            Process(rootNode);
            return new ArrayValue(rootNode.Flatten().Select(o => new ObjectValue(o)));

            void Process(ValueNode<PageData> targetNode)
            {
                if (depth > maxDepth)
                {
                    return;
                }
                depth++;
                foreach (var page in contentLoader.GetChildren<PageData>(targetNode.Value.ContentLink))
                {
                    var node = new ValueNode<PageData>(page);
                    targetNode.AddChildren(node);
                    Process(node);
                }
                depth--;
            }
        }

        /* Helper Methods */
        private static ContentReference GetContentReference(object obj)
        {
            if (obj is ContentReference || obj is PageReference)
                return (ContentReference)obj;

            if (obj is IContent)
                return ((IContent)obj).ContentLink;

            if (obj is int)
                return new ContentReference((int)obj);

            return null;
        }

        private static IEnumerable<ObjectValue> GetAncestors(ContentReference target)
        {
            var results = contentLoader.GetAncestors(target).Select(r => contentLoader.Get<ContentData>(r.ContentLink));
            return results.Select(r => new ObjectValue(r));
        }

        private static IEnumerable<ObjectValue> GetChildren(ContentReference parent)
        {
            var results = contentLoader.GetChildren<ContentData>(parent);
            return results.Select(r => new ObjectValue(r));
        }

    }

    public interface INode
    {
        bool IsOpening { get; }
        bool IsClosing { get; }
        bool HasValue { get; }
        int Depth { get; }
    }

    public class ClosingNode : INode
    {
        public int Depth { get; private set; }
        public ClosingNode(int depth)
        {
            Depth = depth;
        }

        public bool IsOpening => false;
        public bool IsClosing => true;
        public bool HasValue => false;
    }

    public class OpeningNode : INode
    {
        public int Depth { get; private set; }
        public OpeningNode(int depth)
        {
            Depth = depth;
        }

        public bool IsOpening => true;
        public bool IsClosing => false;
        public bool HasValue => false;
    }

    public class ValueNode<T> : INode
    {
        // This is the thing we're actually wrapping
        public T Value { get; set; }

        // These need to be set
        public ValueNode<T> Parent { get; set; }
        public List<ValueNode<T>> Children { get; set; } = new List<ValueNode<T>>();

        // These are calculated
        public bool HasValue => true;
        public bool IsOpening => false;
        public bool IsClosing => false;
        public List<ValueNode<T>> Generation => Parent?.Children ?? new List<ValueNode<T>>();
        public int Ordinal => Parent?.Children.FindIndex(i => i.Value.Equals(Value)) ?? 0;
        public int Depth => Ancestors.Count;
        public bool IsFirstChild => Parent == null || Generation.Count == 1 || Ordinal == 0;
        public bool IsLastChild => Parent == null || Generation.Count == 1 || Ordinal + 1 == Generation.Count;
        public bool HasChildren => Children.Any();

        public ValueNode(T item)
        {
            Value = item;
        }

        public List<ValueNode<T>> Ancestors
        {
            get
            {
                var ancestors = new List<ValueNode<T>>();
                var current = Parent;
                while (true)
                {
                    if (current == null) break;
                    ancestors.Add(current);
                    current = current.Parent;
                }
                return ancestors;
            }
        }

        public void AddChildren(params ValueNode<T>[] children)
        {
            foreach (var child in children)
            {
                child.Parent = this;
                Children.Add(child);
            }
        }

        public List<INode> Flatten()
        {
            var list = new List<INode>();
            list.Add(new OpeningNode(Depth));
            Process(this);
            list.Add(new ClosingNode(Depth));
            return list;

            void Process(ValueNode<T> node)
            {
                list.Add(node);
                if (node.HasChildren)
                {
                    list.Add(new OpeningNode(node.Depth));
                    foreach (var child in node.Children)
                    {
                        Process(child);
                    }
                    list.Add(new ClosingNode(node.Depth));
                }
            }
        }
    }

}
