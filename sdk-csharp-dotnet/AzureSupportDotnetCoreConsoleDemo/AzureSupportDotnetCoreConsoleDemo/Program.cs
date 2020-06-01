/*
 The MIT License (MIT)

Copyright (c) 2020 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.Azure.Management.Support;
using Microsoft.Azure.Management.Support.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Type = Microsoft.Azure.Management.Support.Models.Type;

namespace AzureSupportDotnetCoreConsoleDemo
{
    // This sample console app shows how to create a azure support clint and call various supported operations.
    // Reference: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/supportability?view=azure-dotnet
    class Program
    {
        // For simplicity, manually provide the valid subscription id here. 
        // You can customize the logic by referencing package https://docs.microsoft.com/en-us/dotnet/api/overview/azure/resource-manager?view=azure-dotnet
        private const string SUBID = "<TODO: Replace with your actual subscription ID>";
        private const string TICKETNAMEPREFIX = "ApiDemoConsoleApp_{0}_{1}";
        private const string OPTIONSSUFFIX = " for the subscription " + SUBID;
        private const string ERRORMSG = "\nSome error occured! Please file a github issue if you think there is an issue with the original code.";
        private static CustomLoginCredentials serviceClientCredentials;
        private static MicrosoftSupportClient supportClient;

        static void Main(string[] args)
        {

            // Setup support client and basic configuration like subscription ID
            serviceClientCredentials = new CustomLoginCredentials();
            supportClient = new MicrosoftSupportClient(serviceClientCredentials);
            supportClient.SubscriptionId = SUBID;

            // Setup options
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Welcome to Azure Support sample console app. Make sure to provide auth token and subscription id before running the app!");
            Console.WriteLine("");
            ConsoleKeyInfo option;
            var cycleOptions = true;
            do
            {
                DisplayOptions();
                option = Console.ReadKey(false);
                while(Console.ReadKey(true).Key != ConsoleKey.Enter) { continue; }
                Console.WriteLine("");
                switch (option.KeyChar.ToString())
                {
                    case "1":
                        GetSupportTicketList(filter: "status eq 'Open' and CreatedDate gt " + DateTime.UtcNow.AddDays(-7).ToString("o"));
                        break;

                    case "2":
                        ExecuteOption2();
                        break;

                    case "3":
                        ExecuteOption3();
                        break;

                    case "4":
                        ExecuteOption4();
                        break;

                    case "5":
                        ExecuteOption5();
                        break;

                    case "6":
                        cycleOptions = false;
                        break;

                    default:
                        break;
                }
            } while (option.Key != ConsoleKey.Escape && cycleOptions);
        }

        private static void DisplayOptions()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("=== Create and manage support tickets using Support API ===");
            Console.WriteLine("===========================================================");
            Console.WriteLine("");
            Console.WriteLine("1. Get list of tickets that are in open state from past week"+ OPTIONSSUFFIX);
            Console.WriteLine("2. Create Compute VM cores support ticket and add new communication to the ticket" + OPTIONSSUFFIX);
            Console.WriteLine("3. Create Billing support ticket and update severity"+ OPTIONSSUFFIX);
            Console.WriteLine("4. Create Subscription management support ticket and update additional contact details" + OPTIONSSUFFIX);
            Console.WriteLine("5. Create Technical support ticket for CosmosDb throttling issue" + OPTIONSSUFFIX);
            Console.WriteLine("6. Exit");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.Write("Enter your choice (1-6): ");
            Console.WriteLine("");
            Thread.Sleep(1000);
        }

        // Option 1: Get List of support tickets
        private static void GetSupportTicketList(int? top = null, string filter = null)
        {
            try
            {
                var rsp = supportClient.SupportTickets.List(top, filter).ToList();
                Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        // Option 2: 
        //  1. Call services list api and find quota service name
        //  2. Call problem classification list api and find cores problem classification name
        //  3. Create random ticket name and call check name availability
        //  4. Create support ticket 
        //  5. Create random ticket communication name and call check name availability 
        //  6. Add communication
        //  7. Close the support ticket
        private static void ExecuteOption2()
        {
            try
            {


                //  1.Call services list api and find quota service name
                var rsp1 = GetServiceList();
                var serviceName = string.Empty;
                foreach(var service in rsp1)
                {
                    if (service.DisplayName.ToLower().Contains("service and subscription limits"))
                    {
                        serviceName = service.Name;
                        break;
                    }
                }


                //  2. Call problem classification list api and find cores problem classification name
                var rsp2 = GetProblemClassificationList(serviceName);
                var problemClassificationName = string.Empty;
                foreach (var problemClassification in rsp2)
                {
                    if (problemClassification.DisplayName.ToLower().Contains("compute-vm"))
                    {
                        problemClassificationName = problemClassification.Name;
                        break;
                    }
                }


                //  3.Create random ticket name and call check name availability until unique name is not found
                var rsp3 = true;
                var randomTicketName = string.Empty;
                do
                {
                    randomTicketName = string.Format(TICKETNAMEPREFIX, DateTime.Today.ToString("%d_%M_%y"), new Random().Next(0, 10).ToString());
                    rsp3 = CheckNameAvailability("", new CheckNameAvailabilityInput()
                    {
                        Name = randomTicketName,
                        Type = Type.MicrosoftSupportSupportTickets
                    });
                } while (!rsp3);


                //  4. Create support ticket
                var inputPayload = GenerateCreateSupportTicketPayload();
                inputPayload.ServiceId = "/providers/Microsoft.Support/services/" + serviceName;
                inputPayload.ProblemClassificationId = "/providers/Microsoft.Support/services/" + serviceName + "/problemClassifications/" + problemClassificationName;
                inputPayload.QuotaTicketDetails = new QuotaTicketDetails()
                {
                    QuotaChangeRequestVersion = "1.0",
                    QuotaChangeRequests = new List<QuotaChangeRequest>()
                    {
                        new QuotaChangeRequest()
                        {
                            Region = "EastUS",
                            Payload = "{\"SKU\":\"DSv3 Series\",\"NewLimit\":104}"
                        }
                    }
                };
                CreateSupportTicket(randomTicketName, inputPayload);


                //  5. Create random ticket communication name and call check name availability 
                var randomTicketCommunicationName = randomTicketName + "_communication";
                var rsp5 = CheckNameAvailability(randomTicketName, new CheckNameAvailabilityInput()
                {
                    Name = randomTicketCommunicationName,
                    Type = Type.MicrosoftSupportCommunications
                });


                //  6. Add communication
                CreateSupportTicketCommunication(randomTicketName, randomTicketCommunicationName, new CommunicationDetails()
                {
                    Sender = "abc@contoso.com",
                    Subject = "This is a test ticket",
                    Body = "This is a test ticket communication. Ticket can be closed without any work"
                });


                //  7. Close the support ticket
                UpdateSupportTicketStatus(randomTicketName, "closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        // Option 3: 
        //  1. Call services list api and find billing service name
        //  2. Call problem classification list api and find refund request problem classification name
        //  3. Create random ticket name and call check name availability
        //  4. Create support ticket 
        //  5. Update Severity to moderate
        //  6. Update severity back to minimal and close the support ticket
        private static void ExecuteOption3()
        {
            try
            {


                //  1.Call services list api and find quota service name
                var rsp1 = GetServiceList();
                var serviceName = string.Empty;
                foreach (var service in rsp1)
                {
                    if (service.DisplayName.ToLower().Contains("billing"))
                    {
                        serviceName = service.Name;
                        break;
                    }
                }


                //  2. Call problem classification list api and find cores problem classification name
                var rsp2 = GetProblemClassificationList(serviceName);
                var problemClassificationName = string.Empty;
                foreach (var problemClassification in rsp2)
                {
                    if (problemClassification.DisplayName.ToLower().Contains("refund request"))
                    {
                        problemClassificationName = problemClassification.Name;
                        break;
                    }
                }


                //  3.Create random ticket name and call check name availability until unique name is not found
                var rsp3 = true;
                var randomTicketName = string.Empty;
                do
                {
                    randomTicketName = string.Format(TICKETNAMEPREFIX, DateTime.Today.ToString("%d_%M_%y"), new Random().Next(0, 10).ToString());
                    rsp3 = CheckNameAvailability("", new CheckNameAvailabilityInput()
                    {
                        Name = randomTicketName,
                        Type = Type.MicrosoftSupportSupportTickets
                    });
                } while (!rsp3);


                //  4. Create support ticket
                var inputPayload = GenerateCreateSupportTicketPayload();
                inputPayload.ServiceId = "/providers/Microsoft.Support/services/" + serviceName;
                inputPayload.ProblemClassificationId = "/providers/Microsoft.Support/services/" + serviceName + "/problemClassifications/" + problemClassificationName;
                CreateSupportTicket(randomTicketName, inputPayload);


                //  5. Update Severity to moderate
                UpdateSupportTicketSeverity(randomTicketName, "Moderate");


                //  6. Update severity back to minimal and close the support ticket
                UpdateSupportTicket(randomTicketName, new UpdateSupportTicket()
                {
                    Severity = "Minimal",
                    Status = "closed"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        // Option 4: 
        //  1. Call services list api and find subscription management service name
        //  2. Call problem classification list api and find cancel subscription problem classification name
        //  3. Create random ticket name and call check name availability
        //  4. Create support ticket 
        //  5. Update additional email contact details
        //  6. Close the support ticket
        private static void ExecuteOption4()
        {
            try
            {


                //  1.Call services list api and find quota service name
                var rsp1 = GetServiceList();
                var serviceName = string.Empty;
                foreach (var service in rsp1)
                {
                    if (service.DisplayName.ToLower().Contains("subscription management"))
                    {
                        serviceName = service.Name;
                        break;
                    }
                }


                //  2. Call problem classification list api and find cores problem classification name
                var rsp2 = GetProblemClassificationList(serviceName);
                var problemClassificationName = string.Empty;
                foreach (var problemClassification in rsp2)
                {
                    if (problemClassification.DisplayName.ToLower().Contains("cancel my subscription"))
                    {
                        problemClassificationName = problemClassification.Name;
                        break;
                    }
                }


                //  3.Create random ticket name and call check name availability until unique name is not found
                var rsp3 = true;
                var randomTicketName = string.Empty;
                do
                {
                    randomTicketName = string.Format(TICKETNAMEPREFIX, DateTime.Today.ToString("%d_%M_%y"), new Random().Next(0, 10).ToString());
                    rsp3 = CheckNameAvailability("", new CheckNameAvailabilityInput()
                    {
                        Name = randomTicketName,
                        Type = Type.MicrosoftSupportSupportTickets
                    });
                } while (!rsp3);


                //  4. Create support ticket
                var inputPayload = GenerateCreateSupportTicketPayload();
                inputPayload.ServiceId = "/providers/Microsoft.Support/services/" + serviceName;
                inputPayload.ProblemClassificationId = "/providers/Microsoft.Support/services/" + serviceName + "/problemClassifications/" + problemClassificationName;
                CreateSupportTicket(randomTicketName, inputPayload);


                //  5.Update additional email contact details
                UpdateSupportTicketContact(randomTicketName, new UpdateContactProfile()
                {
                    AdditionalEmailAddresses = new List<string>() { "xyz@contoso.com" }
                });


                //  6. Close the support ticket
                UpdateSupportTicketStatus(randomTicketName, "closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        // Option 5: 
        //  1. Call services list api and find technical cosmos db service name
        //  2. Call problem classification list api and find throttling problem classification name
        //  3. Create random ticket name and call check name availability
        //  4. Create support ticket 
        //  5. Close the support ticket
        private static void ExecuteOption5()
        {
            try
            {


                //  1.Call services list api and find quota service name
                var rsp1 = GetServiceList();
                var serviceName = string.Empty;
                foreach (var service in rsp1)
                {
                    if (service.DisplayName.ToLower().Contains("cosmos db"))
                    {
                        serviceName = service.Name;
                        break;
                    }
                }


                //  2. Call problem classification list api and find cores problem classification name
                var rsp2 = GetProblemClassificationList(serviceName);
                var problemClassificationName = string.Empty;
                foreach (var problemClassification in rsp2)
                {
                    if (problemClassification.DisplayName.ToLower().Contains("throttling"))
                    {
                        problemClassificationName = problemClassification.Name;
                        break;
                    }
                }


                //  3.Create random ticket name and call check name availability until unique name is not found
                var rsp3 = true;
                var randomTicketName = string.Empty;
                do
                {
                    randomTicketName = string.Format(TICKETNAMEPREFIX, DateTime.Today.ToString("%d_%M_%y"), new Random().Next(0, 10).ToString());
                    rsp3 = CheckNameAvailability("", new CheckNameAvailabilityInput()
                    {
                        Name = randomTicketName,
                        Type = Type.MicrosoftSupportSupportTickets
                    });
                } while (!rsp3);


                //  4. Create support ticket
                var inputPayload = GenerateCreateSupportTicketPayload();
                inputPayload.ServiceId = "/providers/Microsoft.Support/services/" + serviceName;
                inputPayload.ProblemClassificationId = "/providers/Microsoft.Support/services/" + serviceName + "/problemClassifications/" + problemClassificationName;
                CreateSupportTicket(randomTicketName, inputPayload);


                //  5. Close the support ticket
                UpdateSupportTicketStatus(randomTicketName, "closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        private static void UpdateSupportTicketSeverity(string supportTicketName, string severity)
        {
            UpdateSupportTicket(supportTicketName, new UpdateSupportTicket()
            {
                Severity = severity
            });
        }

        private static void UpdateSupportTicketStatus(string supportTicketName, string status)
        {
            UpdateSupportTicket(supportTicketName, new UpdateSupportTicket()
            {
                Status = status
            });
        }

        private static void UpdateSupportTicketContact(string supportTicketName, UpdateContactProfile contact)
        {
            UpdateSupportTicket(supportTicketName, new UpdateSupportTicket()
            {
                ContactDetails = contact
            });
        }

        private static void CreateSupportTicket(string supportTicketName, SupportTicketDetails createPayload)
        {
            try
            {
                var rsp = supportClient.SupportTickets.Create(supportTicketName, createPayload);
                Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        private static void UpdateSupportTicket(string supportTicketName, UpdateSupportTicket updatePayload)
        {
            try
            {
                var rsp = supportClient.SupportTickets.Update(supportTicketName, updatePayload);
                Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        private static bool CheckNameAvailability(string supportTicketName, CheckNameAvailabilityInput inputPayload)
        {
            try
            {
                if (inputPayload.Type == Type.MicrosoftSupportSupportTickets)
                {
                    var rsp = supportClient.SupportTickets.CheckNameAvailability(inputPayload);
                    return rsp.NameAvailable ?? false;
                }
                else if (inputPayload.Type == Type.MicrosoftSupportCommunications)
                {
                    var rsp = supportClient.Communications.CheckNameAvailability(supportTicketName, inputPayload);
                    return rsp.NameAvailable ?? false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
                return false;
            }
        }

        private static void CreateSupportTicketCommunication(string supportTicketName, string communicationName, CommunicationDetails createCommunicationPayload)
        {
            try
            {
                var rsp = supportClient.Communications.Create(supportTicketName, communicationName, createCommunicationPayload);
                Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        private static List<Service> GetServiceList()
        {
            return supportClient.Services.List().ToList();
        }

        private static List<ProblemClassification> GetProblemClassificationList(string serviceName)
        {
            return supportClient.ProblemClassifications.List(serviceName).ToList();
        }

        private static SupportTicketDetails GenerateCreateSupportTicketPayload()
        {
            return new SupportTicketDetails()
            {
                Severity = "Minimal",
                Title = "Test ticket from Azure Support sample console app",
                Description = "Test ticket from Azure Support sample console app",
                ContactDetails = new ContactProfile()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    PrimaryEmailAddress = "abc@contoso.com",
                    PreferredContactMethod = "email",
                    PreferredTimeZone = "Pacific Standard Time",
                    PreferredSupportLanguage = "en-US",
                    Country = "usa"
                }
            };
        }
    }
}
