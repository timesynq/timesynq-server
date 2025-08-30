var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<SqlServerDatabaseResource> database = builder.AddSqlServer("database")
    .WithDataVolume()
    .AddDatabase("timesynq-db");

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis")
    .WithRedisInsight();

builder.AddProject<Projects.TimesynqServer>("timesynqserver")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(redis)
    .WaitFor(redis);

builder.Build().Run();
