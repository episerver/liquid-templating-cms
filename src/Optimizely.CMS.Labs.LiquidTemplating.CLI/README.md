<p align="center"><img width="180px" src="https://github.com/episerver/liquid-templating-cms/raw/main/opti-liquid-logo.png"></p>

## opti-liquid-templates cli

The Optimizely Liquid templates cli tool is a portable executable built to demonstrate the development workflow flexibility that the liquid templating option in Optimizely CMS can provide. The cli tool and updates to the core Optimizely.CMS.Labs.LiquidTemplating functionality allows a FE/template developer to:

- Manage liquid templates as IContent within CMS, preferentially overriding liquid templates or razor templates included within the deployment package
- Initialise their working environment by downloading all remote liquid files from the CMS
- Deploy template changes make locally using their development tools of choice, to an Optimizely CMS 12 instance, outside of standard asp.net publishing patterns 
- Preview changes on a fully remote CMS instance

The cli tooling is a multi-platform dotnet executable, allowing it to be used on Mac / Linux and Windows operating systems. The cli tool provides a bridge for a FE developer, meaning that they require no additional local installation of Optimizely or dotnet / SQL server components to be able to develop, preview and deploy template changes to Optimizely CMS. The cli tool can be used locally be individual developers or included as part of a CI pipeline, ensuring that template code can be synchronised with a single source of truth, which could be the CMS itself or more likely a development source code system such as git.

### Usage

```
opti-liquid-templates [mode] [hostname] [clientId] [clientSecret] [path-to-templates]

Synchronize .liquid template files to a remote Optimizely Content Management instance

mode:
	- pull\t\tDownload all remote .liquid template files and folder structure from a remote cms host and save to the specified local folder
    - push\t\tUpload all local .liquid template files and folders to the specified remote cms host
	- watch\t\tWatch for changes in the specified local folder and upload changes to the specified remote cms host

arguments:

	- hostname: The hostname of the remote cms instance. Must have ContentDelivery and ContentManagement api installed with a client grant Auth scheme. Example - https://localhost:5000
	- clientId: The clientId configured to allow access to the remote cms ContentManagement api. Defaults to cli
	- clientSecret: The clientSecret configured to allow access to the remote cms ContentManagement api. Defaults to cli
	- path-to-templates: The relative path to your local templates from the location of this executable. Example - \\liquid-views

```    

### Pre-requisities

The command line requires an accessible Optimizely CMS 12 instance with the Optimizely Liquid Templates package installed. The CMS must have both ContentDelivery and ContentManagement API's installed, with a client_grant authentication scheme. All communication between the cli tool and the CMS instance uses the Content APIs'. Example configuration is made within the Alloy solution within this repo.
   
### Build

The portable .exe file is can be built by opening the project in Visual Studio and using the publish command. Note, first time build can take several minutes as Visual Studio will download the dotnet runtime packages to include. Build is much quicker after this. This will build a standalone .exe file into the /build directory. The tool can be operated by debugging within Visual Studio. Default command line parameters can be modified in the project properties or by editing the launchSettings.json file.

```
{
  "profiles": {
    "liquid-templating-cli": {
      "commandName": "Project",
      "commandLineArgs": "Push https://localhost:5000 cli cli ..\\..\\..\\..\\..\\examples\\liquid-views"
    }
  }
}

```
