using Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;
using RestSharp;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Reader;

public class ContentDeliveryTemplateReader : ILiquidTemplateReader
{
    protected readonly string Hostname;
    protected readonly RestClient Client;

    private const string _baseRoute = "/api/episerver/v3.0";
    private const string _masterLanguage = "en";
    private string _viewsRootGuid = "088d483d-aa5d-408f-8fa8-9d6cda9da03f";

    public ContentDeliveryTemplateReader(string hostname)
    {
        Hostname = hostname;
        Client = new RestClient(Hostname + _baseRoute);
    }

    public ILiquidTemplateData? Get(string contentReference)
    {
        string resource = "/content/" + contentReference;
        var request = new RestRequest(resource, Method.Get);
        request.AddHeader("Accept-Language", _masterLanguage);
        request.AddQueryParameter("select", "template");

        var response = Client.Execute<ContentResponse>(request);
        var contentResponse = response?.Data;
        if (contentResponse == null)
            return null;

        return new RemoteLiquidTemplateData(contentResponse) { Hierarchy = contentResponse.name, Template = contentResponse.template };    
    }

    public List<ILiquidTemplateData> GetAll()
    {
        var root = Get(_viewsRootGuid);
        var results = new List<ILiquidTemplateData>();
        if (root == null)
            return results;

        FindDescendants(root, results);

        return results;
    }

    private void FindDescendants(ILiquidTemplateData data, IList<ILiquidTemplateData> templateDataMaps)
    {
        string resource = "/content/" + data.ContentId + "/children";
        var request = new RestRequest(resource, Method.Get);
        request.AddHeader("Accept-Language", _masterLanguage);
        request.AddQueryParameter("select", "template");

        var response = Client.Execute<List<ContentResponse>>(request);

        var children = response.Data;
        if (children == null)
            return;

        foreach (var contentResponse in children)
        {
            var childMap = new RemoteLiquidTemplateData(contentResponse) 
            {
                Hierarchy = data.Key + "|" + contentResponse.name,
                Template = contentResponse.template
            };

            templateDataMaps.Add(childMap);
            FindDescendants(childMap, templateDataMaps);
        }
    }
}