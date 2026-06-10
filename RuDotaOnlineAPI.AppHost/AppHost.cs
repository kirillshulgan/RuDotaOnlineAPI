var builder = DistributedApplication.CreateBuilder(args);

// ── Секреты ───────────────────────────────────────────────────────────────────
var jwtPrivateKey = builder.AddParameter("jwt-private-key", secret: true);
var jwtPublicKey = builder.AddParameter("jwt-public-key", secret: true);
var steamApiKey = builder.AddParameter("steam-api-key", secret: true);

// ── PostgreSQL ────────────────────────────────────────────────────────────────
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume("postgres_data")
    .WithBindMount("./scripts/postgres-init.sh",
        "/docker-entrypoint-initdb.d/init.sh");

var identityDb = postgres.AddDatabase("myapp-identity", "Identity_dev");
var gameDb = postgres.AddDatabase("myapp-game", "Game_dev");

// ── Redis ─────────────────────────────────────────────────────────────────────
var redisPassword = builder.AddParameter("redis-password", secret: true);

var redis = builder.AddRedis("redis", password: redisPassword)
    .WithDataVolume("redis_data");

// ── RabbitMQ ──────────────────────────────────────────────────────────────────
var rabbitUser = builder.AddParameter("rabbitmq-user");
var rabbitPassword = builder.AddParameter("rabbitmq-password", secret: true);

var rabbit = builder.AddRabbitMQ("rabbitmq",
        userName: rabbitUser,
        password: rabbitPassword)
    .WithManagementPlugin()
    .WithDataVolume("rabbitmq_data")
    .WithEnvironment("RABBITMQ_DEFAULT_VHOST", "rudota");

// ── Identity API ──────────────────────────────────────────────────────────────
var identityApi = builder.AddProject<Projects.RuDotaOnlineAPI_Service_Identity>("identity-api")
    .WithReference(identityDb)
    .WithReference(redis)
    .WithReference(rabbit)
    .WaitFor(identityDb)
    .WaitFor(redis)
    .WaitFor(rabbit)
    .WithEnvironment("RabbitMq__VirtualHost", "rudota")
    .WithEnvironment("Jwt__PrivateKeyPem", jwtPrivateKey)
    .WithEnvironment("Jwt__PublicKeyPem", jwtPublicKey)
    .WithEnvironment("Steam__ApiKey", steamApiKey)
    .WithEnvironment("Steam__CallbackUrl",
        "http://localhost:5000/api/v1/auth/steam/callback");

// ── Gateway ───────────────────────────────────────────────────────────────────
builder.AddProject<Projects.RuDotaOnlineAPI_Gateway>("gateway")
    .WithReference(identityApi)
    .WaitFor(identityApi)
    .WithEnvironment("ReverseProxy__Clusters__identity-cluster__Destinations__identity-api__Address",
        identityApi.GetEndpoint("https"));

builder.Build().Run();