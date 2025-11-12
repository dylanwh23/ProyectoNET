using Microsoft.Identity.Client.Extensions.Msal;

var builder = DistributedApplication.CreateBuilder(args);
/**/
//base de datos 
var postgresServer = builder.AddPostgres("postgres-server");
postgresServer.WithDataVolume("postgres_data");
var dbCarreraAPI = postgresServer.AddDatabase("carreras-db");
var dbUsuariosAPI = postgresServer.AddDatabase("usuarios-db");
//storage de imagenes
var blobStorage = builder.AddAzureStorage("storage")
                            .RunAsEmulator(options => { options.WithBlobPort(10000); options.WithDataVolume("blob_data"); } )
                            .AddBlobs("blobstorage");

//rabbi
var rabbitmq = builder.AddRabbitMQ("rabbitmq-bus").WithManagementPlugin()
    .WithDataVolume("rabbitmq_data");
              

var carrerasApi = builder.AddProject<Projects.ProyectoNET_Carreras_API>("carreras-api")
       .WithReference(rabbitmq)
       .WithReference(dbCarreraAPI)
       .WithReference(blobStorage);
var usuariosApi = builder.AddProject<Projects.ProyectoNET_Usuarios_API>("usuarios-api")
       .WithReference(rabbitmq)
       .WithReference(dbUsuariosAPI);
//worker
builder.AddProject<Projects.ProyectoNET_SimulatorWorker>("simulator-worker")
       .WithReference(rabbitmq); 
//front       
builder.AddProject<Projects.ProyectoNET_WebApp>("webapp")
       .WithReference(carrerasApi)  
       .WithReference(usuariosApi)
       .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
//front admin     
builder.AddProject<Projects.ProyectoNET_AdminWebApp>("admin-webapp")
       .WithReference(carrerasApi)
       .WithReference(usuariosApi)
       .WithHttpEndpoint(port: 7072);


builder.Build().Run();
