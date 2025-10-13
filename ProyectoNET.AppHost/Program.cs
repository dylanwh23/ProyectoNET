var builder = DistributedApplication.CreateBuilder(args);
/**/
//rabbit
var rabbitmq = builder.AddRabbitMQ("rabbitmq-bus");
//microservicios
var carrerasApi = builder.AddProject<Projects.ProyectoNET_Carreras_API>("carreras-api")
       .WithReference(rabbitmq);
var usuariosApi = builder.AddProject<Projects.ProyectoNET_Usuarios_API>("usuarios-api");
//worker
builder.AddProject<Projects.ProyectoNET_SimulatorWorker>("simulator-worker")
       .WithReference(rabbitmq); 
//front       
builder.AddProject<Projects.ProyectoNET_WebApp>("webapp")
       .WithReference(carrerasApi)  
       .WithReference(usuariosApi)
       .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
//front admin     
builder.AddProject<Projects.ProyectoNET_WebApp>("admin-webapp")
       .WithReference(carrerasApi)  
       .WithReference(usuariosApi);    
/**/
builder.Build().Run();
