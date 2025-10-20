using Projects;

var builder = DistributedApplication.CreateBuilder(args);
/**/
//base de datos 
var postgresServer = builder.AddPostgres("postgres-server");
postgresServer.WithDataVolume("postgres_data");
var dbCarreraAPI = postgresServer.AddDatabase("carreras-db");
var dbUsuariosAPI = postgresServer.AddDatabase("usuarios-db"); 
//rabbit
var rabbitmq = builder.AddRabbitMQ("rabbitmq-bus");
//microservicios
var carrerasApi = builder.AddProject<Projects.ProyectoNET_Carreras_API>("carreras-api")
       .WithReference(rabbitmq)
       .WithReference(dbCarreraAPI);
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
