---
page_type: sample
languages:
- csharp
products:
- dotnet
description: "Add 150 character max description"
urlFragment: "update-this-to-unique-url-stub"
---

# Official Microsoft Sample

<!-- 
Guidelines on README format: https://review.docs.microsoft.com/help/onboard/admin/samples/concepts/readme-template?branch=master

Guidance on onboarding samples to docs.microsoft.com/samples: https://review.docs.microsoft.com/help/onboard/admin/samples/process/onboarding?branch=master

Taxonomies for products and languages: https://review.docs.microsoft.com/new-hope/information-architecture/metadata/taxonomies?branch=master
-->

This reposistory contains one or more sample projects/code that uses one of azure support api interfaces that are publicly available. (e.g. c# sdk)

## Contents

High level description of contents in this repository

| File/folder       | Description                                |
|-------------------|--------------------------------------------|
| `sdk-csharp-dotnet`             | Sample source using c# sdk based on .net core.                        |
| `.gitignore`      | Define what to ignore at commit time.      |
| `CHANGELOG.md`    | List of changes to the sample.             |
| `CONTRIBUTING.md` | Guidelines for contributing to the sample. |
| `README.md`       | This README file.                          |
| `LICENSE`         | The license for the sample.                |

## Prerequisites

### sdk-csharp-dotnet

1. IDE like visual studio that can build .net core based c# projects.
2. ARMClient https://github.com/projectkudu/ARMClient or any other equivalent code that would help generate auth token for arm rest api.
3. Subscription ID

## Setup

### sdk-csharp-dotnet 

1. Open the solution file in your IDE.
2. Search for couple of `<TODO:` in the code and replace them with appropriate values based on the guidance in the comments.
3. Run the nuget restore in the project directory (https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-restore) from command line orusing IDE's integrated options. 
4. Build the solution and make sure there were no build errors.

## Running the sample

### sdk-csharp-dotnet

You can either run `release` version of the binaries or do the live debugging using `debug` version of the compiled code.

For running release variant binaries, you would need to package them correctly and run on any operating system. For more information refer https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run

For testing the code, you can run debug variant binaries from your IDE, set up break point and do step by step debugging as needed.

## Key concepts

### sdk-csharp-dotnet 

This sample is a console app built using .net core 3.1 in C# language. It mainly contains two source files. `CustomLoginCredentials` contains high level auth logic that is used with every call made by the client. `Program.cs` contains the logic of creating a single client for Microsoft.Azure.Management.Support SDK and calling appropriate support api management operations based on the console option selection from the user. You can refer to     // Reference: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/supportability?view=azure-dotnet
for more information on supported operations in our sdk.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
