using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var statestore = builder.AddDaprStateStore("skyquerystatestore");
var pubsubComponent = builder.AddDaprPubSub("pubsub");

builder.AddProject<Projects.SkyQuery_ImageService>("skyquery-imageservice")
        .WithDaprSidecar(new DaprSidecarOptions
        {
            AppId = "skyquery-imageservice-dapr",
            DaprHttpPort = 3600
        })
        .WithReference(statestore).WithReference(pubsubComponent);

builder.AddProject<Projects.SkyQuery_AppGateway>("skyquery-appgateway")
        .WithDaprSidecar(new DaprSidecarOptions
        {
            AppId = "skyquery-appgateway-dapr",
            DaprHttpPort = 3601
        })
        .WithReference(statestore).WithReference(pubsubComponent);


builder.AddProject<Projects.SkyQuery_AuthService>("skyquery-authservice")
        .WithDaprSidecar(new DaprSidecarOptions
        {
            AppId = "skyquery-authservice-dapr",
            DaprHttpPort = 3602
        })
        .WithReference(statestore).WithReference(pubsubComponent);

builder.AddProject<Projects.SkyQuery_Website>("skyquery-website");


builder.Build().Run();
