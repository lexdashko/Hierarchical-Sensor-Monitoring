using System;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using HSMServer.Configuration;
using HSMServer.Constants;
using HSMServer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;

namespace HSMServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            CertificatesConfig.InitializeConfig();

            try
            {
                logger.Debug("init main");
                var host = CreateHostBuilder(args).Build();

                StartSignalRService(host);
                
                host.Run();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Program stopped because of an exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return  Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.ConfigureHttpsDefaults(httpsOptions =>
                        {
                            httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                        });
                        options.Listen(IPAddress.Any, ConfigurationConstants.GrpcPort, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                            //listenOptions.UseHttps(Config.ServerCertificate);
                            listenOptions.UseHttps(portOptions =>
                            {
                                portOptions.ServerCertificate = CertificatesConfig.ServerCertificate;
                                portOptions.ClientCertificateValidation = ValidateClientCertificate;
                                portOptions.SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
                            });
                        });
                        options.Listen(IPAddress.Any, ConfigurationConstants.SensorsPort, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                            listenOptions.UseHttps(portOptions =>
                            {
                                portOptions.CheckCertificateRevocation = false;
                                portOptions.SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
                                portOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                                portOptions.ServerCertificate = CertificatesConfig.ServerCertificate;
                            });
                        });
                        options.Listen(IPAddress.Any, ConfigurationConstants.ApiPort, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                            listenOptions.UseHttps(portOptions =>
                            {
                                portOptions.CheckCertificateRevocation = false;
                                portOptions.SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
                                portOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                                //portOptions.ServerCertificate = Config.ServerCertificate;
                            });
                        });
                        options.Limits.MaxRequestBodySize = 41943040;//Set up to 40 MB
                    });
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    logging.AddNLog();
                }).UseNLog().UseConsoleLifetime();
        }

        private static void StartSignalRService(IHost host)
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var serviceContext = services.GetRequiredService<IClientMonitoringService>();
                serviceContext.Initialize();
            }
        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.ConfigureKestrel(options =>
        //            {
        //                options.ConfigureHttpsDefaults(httpsOptions =>
        //                {
        //                    httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        //                });
        //                options.Listen(IPAddress.Any, ConfigurationConstants.GrpcPort, listenOptions =>
        //                {
        //                    listenOptions.Protocols = HttpProtocols.Http2;
        //                    //listenOptions.UseHttps(Config.ServerCertificate);
        //                    listenOptions.UseHttps(portOptions =>
        //                    {
        //                        portOptions.ServerCertificate = Config.ServerCertificate;
        //                        portOptions.ClientCertificateValidation = ValidateClientCertificate;
        //                        portOptions.SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
        //                    });
        //                });
        //                options.Listen(IPAddress.Any, ConfigurationConstants.SensorsPort, listenOptions =>
        //                {
        //                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        //                    listenOptions.UseHttps(portOptions =>
        //                    {
        //                        portOptions.CheckCertificateRevocation = false;
        //                        portOptions.SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
        //                        portOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
        //                        portOptions.ServerCertificate = Config.ServerCertificate;
        //                    });
        //                });
        //                options.Listen(IPAddress.Any, ConfigurationConstants.ApiPort, listenOptions =>
        //                {
        //                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        //                    listenOptions.UseHttps(portOptions =>
        //                    {
        //                        portOptions.CheckCertificateRevocation = false;
        //                        portOptions.SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
        //                        portOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
        //                        portOptions.ServerCertificate = Config.ServerCertificate;
        //                    });
        //                });
        //                options.Limits.MaxRequestBodySize = 41943040;//Set up to 40 MB
        //            });
        //            webBuilder.UseStartup<Startup>();
        //        })
        //        .ConfigureLogging(logging =>
        //        {
        //            logging.ClearProviders();
        //            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        //        }).UseNLog().UseConsoleLifetime();

        public static bool ValidateClientCertificate(X509Certificate2 certificate, X509Chain chain,
            SslPolicyErrors policyErrors)
        {
            return true;
        }
    }
}
