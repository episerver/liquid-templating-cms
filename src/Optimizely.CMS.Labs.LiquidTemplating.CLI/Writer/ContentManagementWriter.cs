using Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;
using Optimizely.CMS.Labs.LiquidTemplating.CLI.Reader;
using RestSharp;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI.Writer;

public class ContentManagementWriter : ILiquidTemplateWriter
{
    private readonly ContentApiAuthenticator _apiAuthenticator;
    private readonly ILiquidTemplateReader _reader;
    
    private const string _baseRoute = "/api/episerver/v3.0/contentmanagement";
    private const string _masterLanguage = "en";

    protected readonly RestClient Client;

    public ContentManagementWriter(string hostname, ContentApiAuthenticator apiAuthenticator, ILiquidTemplateReader reader)
    {
        _apiAuthenticator = apiAuthenticator;
        _reader = reader;
        Client = new RestClient(hostname + _baseRoute);
    }

    public void Create(IEnumerable<ILiquidTemplateData> items)
    {
        foreach (ILiquidTemplateData item in items)
        {
            Create(item);
        }
    }

    public void Create(ILiquidTemplateData item)
    {
        //work out parent
        var parentKey = string.Join('|', item.Key.Split('|').SkipLast(1));
        var parentItem = _reader.GetAll().FirstOrDefault(i => i.Key == parentKey);
        var parentId = int.Parse(parentItem.ContentId);

        var request = new RestRequest(string.Empty, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Bearer " + _apiAuthenticator.GetAuthorizationToken());
        request.AddHeader("x-epi-validation-mode", "minimal");

        var folderRequest = new CreateFolderRequest();
        folderRequest.name = item.Name;
        folderRequest.parentLink = new ContentLink { id = parentId };
       
        if (item.LiquidContentType == LiquidContentItem.Folder)
        {
            folderRequest.contentType = new List<string>() { "SysContentFolder" };
            request.AddBody(folderRequest);
        }
        else
        {
            var contentRequest = new CreateContentRequest(folderRequest);
            contentRequest.contentType = new List<string>() { "LiquidTemplateData" };
            contentRequest.language = new Language() { name = _masterLanguage };
            //contentRequest.template = new Template() { value = item.Template };
            contentRequest.template = item.Template;
            contentRequest.status = "published";
            request.AddBody(contentRequest);
        }

        var response = Client.Execute(request);

        if (response != null)
        {
            if ((int)response.StatusCode >= 400)
            {
                throw new Exception($"Data could not be created with status {response.StatusCode.ToString()}");
            }
        }
    }

    public void Update(IEnumerable<ILiquidTemplateData> items)
    {
        foreach (ILiquidTemplateData item in items)
        {
            Update(item);
        }
    }

    public void Update(ILiquidTemplateData item)
    {
        //work out content Id
        var allLocalItems = _reader.GetAll().ToList();
        var contentItem = allLocalItems.FirstOrDefault(i => i.Key == item.Key);
        var contentId = int.Parse(contentItem.ContentId);
 
        string resource = "/" + contentId;
        var request = new RestRequest(resource, Method.Patch);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Bearer " + _apiAuthenticator.GetAuthorizationToken());
        request.AddHeader("x-epi-validation-mode", "minimal");

        if (item.LiquidContentType == LiquidContentItem.LiquidTemplateData)
        {
            var updateRequest = new UpdateContentRequest
            {
                name = item.Name,
                routeSegment = item.Name,
                template = item.Template
            };
            request.AddBody(updateRequest);
        }
        else
        {
            var updateRequest = new UpdateFolderRequest
            {
                name = item.Name,
                routeSegment = item.Name
            };
            request.AddBody(updateRequest);
        }
        
        var response = Client.Execute(request);

        if (response != null)
        {
            if ((int)response.StatusCode >= 400)
            {
                throw new Exception($"Data could not be updated with status {response.StatusCode.ToString()}");
            }
        }
    }


    public void Delete(IEnumerable<ILiquidTemplateData> items)
    {
        foreach (ILiquidTemplateData item in items)
        {
            Delete(item);
        }
    }

    public void Delete(ILiquidTemplateData item)
    {
        string resource = "/" + item.ContentId;
        var request = new RestRequest(resource, Method.Delete);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Bearer " + _apiAuthenticator.GetAuthorizationToken());
        request.AddHeader("x-epi-permanent-delete", false);

        var response = Client.Execute(request);

        if (response != null)
        {
            if ((int)response.StatusCode >= 400)
            {
                throw new Exception($"Data {item.ContentId} could not be deleted with status {response.StatusCode.ToString()}");
            }
        }
    }
}
