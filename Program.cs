using Microsoft.EntityFrameworkCore;
using minimal_api_swagger.Data;
using minimal_api_swagger.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(x => x.UseInMemoryDatabase("minimal-api-swagger-db"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/v1/clientes", async (AppDbContext context) => await context.Clientes.ToListAsync())
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Obter informações de todos os clientes.",
        Description = "Este método recupera as informações de todos os clientes."
    });

app.MapGet("/v1/clientes/{id}", async (int id, AppDbContext context) => await context.Clientes.FindAsync(id) is Cliente cliente ? Results.Ok(cliente) : Results.NotFound())
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Obter informações de um cliente específico.",
        Description = "Este método recupera as informações de um cliente específico com base no parâmetro 'Id' fornecido."
    })
    .WithOpenApi(generatedOperation =>
    {
        var parameter = generatedOperation.Parameters[0];
        parameter.Description = "'Id' do cliente que deseja recuperar as informações.";

        return generatedOperation;
    });

app.MapPost("/v1/clientes", async (Cliente model, AppDbContext context) =>
    {
        context.Clientes.Add(model);
        await context.SaveChangesAsync();

        return Results.Created($"/clientes/{model.Id}", model);
    })
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Criar um novo cliente.",
        Description = "Este método cria um novo cliente com base nos dados fornecidos. Retorna o Id e as informações do novo cliente criado."
    });

app.MapPut("/v1/clientes/{id}", async (int id, Cliente model, AppDbContext context) =>
    {
        var cliente = await context.Clientes.FindAsync(id);
        if (cliente is null) return Results.NotFound();

        cliente.Nome = model.Nome;
        cliente.Email = model.Email;

        await context.SaveChangesAsync();

        return Results.Ok(cliente);
    })
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Atualizar as informações de um cliente existente.",
        Description = "Este método atualiza as informações de um cliente existente com base no parâmetro 'Id' fornecido e nos dados fornecidos. Retorna as informações atualizadas do cliente."
    })
    .WithOpenApi(generatedOperation =>
    {
        var parameter = generatedOperation.Parameters[0];
        parameter.Description = "'Id' do cliente que deseja atualizar as informações.";

        return generatedOperation;
    });

app.MapDelete("/v1/clientes/{id}", async (int id, AppDbContext context) =>
    {
        if (await context.Clientes.FindAsync(id) is Cliente cliente)
        {
            context.Clientes.Remove(cliente);
            await context.SaveChangesAsync();
            
            return Results.Ok(cliente);
        }
        return Results.NotFound();
    })
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Excluir um cliente.",
        Description = "Este método exclui um cliente específico com base no parâmetro 'Id' fornecido. Esta operação é irreversível e removerá permanentemente todos os dados do cliente. Retorna as informações do cliente excluído."
    })
    .WithOpenApi(generatedOperation =>
    {
        var parameter = generatedOperation.Parameters[0];
        parameter.Description = "'Id' do cliente que deseja excluir.";

        return generatedOperation;
    });

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
