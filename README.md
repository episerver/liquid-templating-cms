<p align="center"><img width="150px" src="https://github.com/made-to-engage/EPiServer.Labs.Liquid/raw/develop/opti-liquid-logo.png"></p>

## Optimizely Liquid Templates

This repository contains an implementation of the Liquid templating language for Optimizely Content Cloud (v12) on dotnet 5 and 6.

[Liquid](https://shopify.dev/api/liquid) is a templating language developed by Shopify that has now become one of the most popular templating languages on the web. It is highly likely that a Front End / User Interface developer will have come across or worked with Liquid at some point during their career. 

This project uses a dotnet Liquid port called [Fluid](https://github.com/sebastienros/fluid). 

The goals of this add-on are to 
- Allow FE Developers to use a Liquid-compatible templating language to perform token replacement, conditionals, and looping operations. 
- Allow FE Developers to create and edit templates in a plain text editor / tool of choice and not be dependent on Visual Studio
- Provide the FE Developer with Liquid-compatible replacements for all the functions of Optimizely Content Cloud's Razor HTML helper’s library (those enabling On-Page-Edit etc.) 

---
## Setup

To use Liquid as a templating language within your Optimizely Content Cloud solution you need to

1. Install the Optimizely.CMS.Labs.LiquidTemplating Nuget package
2. Include the chained AddFluid() and AddCMSFluid() mvc builder calls within StartUp.cs, which configures the Fluid add-on in a specific way for usage within Optimizely CMS. 
3. Write your views in the Liquid templating language in files with a .liquid extension

The above configuration sets up the Fluid plugin and registers an additional ViewEngine which suports the location and finding of .liquid views. Note. The Fluid ViewEngine is added alongside the default MVC Razor ViewEngine, so it is possible to use both templating technologies together should you wish. Optionally you can reconfigure some of the Fluid conventions, for example if you wish to modify view lookup locations or to add your own Fluid tags and functionality.

#### Example configuration (Startup.cs)
```csharp
public void ConfigureServices(IServiceCollection services)
{
...
services.AddMvc().AddFluid().AddCmsFluid();
...
}
```

A Liquid implementation of Alloy is provided in this repository demonstrating the above configuration.

---
## Prerequisites
The following guides will give you a great background into both Liquid and Fluid (the dotnet port of Liquid). They explain how to further extend Fluid, in the event you wanted to add custom features and helper methods to those already provided in this add-on.

- Shopify Liquid Reference (https://shopify.dev/api/liquid)
- Dive into Fluid - (https://deanebarker.net/tech/fluid/)

---
## Features

The below sections describes the Optimizely Content Cloud specific extensions to the Liquid language which enable common CMS functionality to be achieved using Liquid. 

### Rendering Context 
The following custom data and functionality is available within the rendering context of each view.

- CmsContext
- CmsHelper
- ContentLoader
- Property
- ViewContext 

Additionally the Model passed from your controller, or specified within a parent view is available

#### Example usage
```html
<h1>{{ Model.Name }}</h1>
```

The above would render the Name field from an IContent object within an H1 tag, on the assumption the Model passed to the view is of type IContent (for example PageData).

#### CmsContext
The CmsContext object provides access to the CMS context of the current request and resolved content.

#### Properties

  - bool IsInEditMode - whether ther current request originates from within the CMS editor
  - bool IsInReadOnlyMode - whether the CMS is configured to operate in database ReadOnly mode
  - CultureInfo Culture - the resolved language and culture for the current request 
  - ContentReference StartPage - the start page for the currently resolved website
  - ContentReference RootPage - the reference to the content respository root page. 

#### Example usage
```html
{% if CmsContext.IsInEditMode %}
    {% assign method = "POST" %}
{% endif %}

{% assign navigationPages = ContentLoader.GetChildren(CmsContext.StartPage) %}
```

#### CmsHelper
The CmsHelper object provides access to the helper methods for common CMS functionality.

#### Properties

  - CanonicalLink - renders the canonical link for the current page content 
  - AlternateLinks - renders href lang / alternative link elements for multilingual versions of the current page content.
  - EPiServerQuickNavigator - renders the Episerver quick navigation toobar shown to authenticated editors and administrators
  - EditAttributes - renders html attributes allowing the CMS content editing view [requires](https://docs.developers.optimizely.com/content-cloud/v12.0.0-content-cloud/docs/adding-editing-attributes-using-property-web-control) to understand what content property this HTML represents
  - FullRefreshPropertiesMetaData - renders information telling the content editoing view which cpontent property require a full refresh of the page if they are changed. 
  - RequiredClientResources - renders [required resources](https://docs.developers.optimizely.com/content-cloud/v12.0.0-content-cloud/docs/client-resources#mvc-helper) (e.g. css or js files) to the specified area. the start page for the currently resolved website
  - Translate - Renders a translated resource located with the provided key, using the current calculated culture
  - TranslateWithFallback -  Renders a translated resource located with the provided key fallking back to the required text if the resource of translated value isn't available
  - Debug - reflects and renders all available properties and members of the passed object. Useful for FED developers to understand property names and inspect the data available within the Model passed for rendering

#### Example usage
```html
{{ CmsHelper.CanonicalLink() }}
{{ CmsHelper.AlternateLinks() }}
{{ CmsHelper.EpiserverQuickNavigator() }}
{{ CmsHelper.FullRefreshPropertiesMetadata("PropertyName1, PropertyName2") }}
{{ CmsHelper.RequiredClientResources("Header") }}
<h3>{{ CmsHelper.Translate("/footer/company") }}</h3>
<a href="Logout">{{ CmsHelper.TranslateWithFallback("/footer/logout","Logout") }}</a>
{{ CmsHelper.Debug(Model.CurrentPage) }}
```

### ContentLoader
A value allowing you to query the Optimizely Content Repository and retrieve IContent objects

This value provided access to all publically accessible methods of the Optimizely ContentLoader. This is the normal way C# way of loading data from Optimizely. It is a class all Optimizely CMS developers will be familiar with. Additionally some helper methods are avilable that perform common lookup type operations.

#### Properties
The following ContentLoader method are supported:

	- Get(ContentReference contentLink, optional:string language) - returns an ContentData object representing the content data for the requested reference #
	- GetChildren(ContentReference contentLink, optional:string language) - returns an Array of ContentData objects for all child content of the requested reference
	- GetAncestors(ContentReference contentLink, optional:bool reverse) - returns an array of ContentData objects for ancestors of the requested reference
	- GetDescendants(ContentReference contentLink) - returns an array of ContentData objects for descendants of the requested reference

The following helper functions are available:

	- GetStartPage(string language) - returns a PageData object representing the StartPage. language is an optional parameter
	- GetParent(ContentReference contentLink) - returns a ContentData object representing the StartPage. language is an optional parameter
	- GetTopLevelPages() - returns an array of ContentData objects for direct children of the resolved StartPage
	- GetNearestOfType(ContentReference contentLink, string typeName) - returns an array of PageData objects with a PageTypeName that match typeName
	- GetNearestWithValue(ContentReference contentLink, string propertyName) - returns an array of PageData objects with a non null property called propertyName
	- GetDepth(ContentReference contentLink) - returns an integer representing the hierarchical level of the provided ContentReference
	- GetSiblings(ContentReference contentLink, optional:bool includeTarget) -  returns an Array of ContentData objects for all child content of the requested reference
	- GetCrumbtrail(ContentReference contentLink, optional:bool includeHome, optional:bool includeCurrent) - returns an array of ContentData objects for ancestors of the requested reference. optionally including the target and StartPage
	- GetBranch(ContentReference contentLink, optional:int maxDepth, optional:bool includeRootnode) - returns an array of ContentData objects for every branch beneath the requested reference. optionally including the RootPage

All of the Helper methods are overloaded allowing either a ContentReference, IContent or int to be provided to act as the reference. The methods will resolve the required ContentReference in each case.

#### Example usage
```html
<ul class="nav nav-pills flex-column">
	{% assign children = ContentLoader.GetChildren(page.ContentLink) | has_template %}
	{% for child in children %}
		<li class="nav-item">
			<a href="{{ContentLoader.Get(child.ContentLink) | url}}" class="nav-link">{{ ContentLoader.Get(child.ContentLink).Name  }}</a>
		</li>
	{% endfor %}
</ul>

{{ ContentLoader.Get(Model.CurrentPage.ContentLink).Name }}
{{ ContentLoader.Get(Model.CurrentPage.ContentLink, "fr").Name }}
{% for page in ContentLoader.GetChildren(Model.CurrentPage) %}
{% for page in ContentLoader.GetChildren(Model.CurrentPage, "en") %}
{% for page in ContentLoader.GetAncestors(Model.CurrentPage.ContentLink, true) %}
{% for page in ContentLoader.GetDescendants(Model.CurrentPage.ContentLink) %}

{{ ContentLoader.GetStartPage().Name }}
{{ ContentLoader.GetStartPage("fr").Name }}     
{{ ContentLoader.GetParent(Model.CurrentPage).Name }}
{{ ContentLoader.GetParent(Model.CurrentPage.ContentLink).Name }}
{{ ContentLoader.GetParent(26).Name }}
{% for page in ContentLoader.TopLevelPages() %}
{% for page in ContentLoader.GetNearestOfType(Model.CurrentPage, "ProductPage") %}
{% for page in ContentLoader.GetNearestWithValue(Model.CurrentPage.ContentLink, "MetaDescription") %}
{% if ContentLoader.GetDepth(Model.CurrentPage) > 1 %}
{% for page in ContentLoader.GetSiblings(67, true) %}
{% for page in ContentLoader.GetCrumbtrail(Model.CurrentPage) %}
{% for page in ContentLoader.GetBranch(82) %}
                           
```

### Property.For
This renders a CMS property and will add in Edit mode attributes ensuring that On page editing features within the CMS editor works

#### Arguments
  - Model - The Property you wish to Render
  - PropertyName - The name of the property. This is required to allow On-Page Edit to know which property is editable
  - CssClass - adds a css class for the wrapping element. Is not added when rendered in edit mode
  - Tag - specified an alternate display or display options to be used
  - CustomTag - changes the type of element that wrapes the rendered property when rendered in edit mode.
  - ViewData - sets a true flag to the ViewData named element
	

#### Example usage
```html
{{ Property.For(Model.CurrentPage.MainBody, "MainBody") }}
{{ Property.For(Model=Model.CurrentPage.MainContentArea, PropertyName="MainContentArea", CssClass="row") }}
{{ Property.For(Model=Model.CurrentPage.RelatedContentArea, PropertyName="RelatedContentArea", ViewData="Aside") }}
```

By Convention, if up to two arguments are specified then the first is assumed as the Model and second the PropertyName. If any additional arguments are specified then they should be explicitly named.

---

### Filters
Liquid filters are used to modify the output of variables and objects. The following Optimizely specific CMS filters are available


#### Content Filters
The following filters act on Arrays of IContent, remving elements according to the particular filter used

	- access - filters content that the current user does not have the required access to - defaults to AccessLevel.Read. Optional - the required access level ("read", "create", "delete", "administer", "edit", "publish", "noaccess", "fullaccess")
	- for_visitor - filters content to those that are published, the user has read access to, and that have a template / renderer
    - published - filters content to those are published 
    - has_template - filters content to those that have a template / renderer
    - of_type - filters content based on the content type, which is looked up by Content Type Display Name
    - visible_in_menu - filters page data content that has Visible in Menu flag set to true 

#### Example usage
```html
{% assign ancestors = ContentLoader.GetAncestors(Model.ContentLink, true) | has_template %}
{% assign children = ContentLoader.GetChildren(Model.Section.ContentLink) | for_visitor %}
{% assign children = ContentLoader.GetChildren(Model.Section.ContentLink) | access:"Read" %}
```

#### Content Reference Filters
The is_empty filter acts on ContentReference objects and returns true if the ContentReference is null or empty.

#### Example usage
```html
{% assign isImageEmpty = page.PageImage | is_empty %}
```

#### Content Type Filters
The is_true filter acts on IContent objects and returns true if the IContent matches the specified type.

#### Example usage
```html
{% assign isStartPage = page | is_type:"StartPage" %}
```
#### Url Filters
The url filter acts on IContent, ContentReference, Url objects or strings and returns a string value representing the external url to the specified object. 

#### Example usage
```html
<img src="{{ Model.CurrentPage.PageImage | url }}" />
<li class="breadcrumb-item"><a href="{{ContentLoader.Get(parent.ContentLink) | url}}">{{ ContentLoader.Get(parent.ContentLink).Name  }}</a></li>
<a href="{{ Model.ContentLink | url }}" title="{{Model.Name}}">	
```
