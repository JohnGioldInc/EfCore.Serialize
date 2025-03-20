Current issue with Take and Skip functions

System.Collections.Generic.KeyNotFoundException: The given key '__p_0' was not present in the dictionary.
   at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
   at lambda_method102(Closure, QueryContext)
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.ExecuteCore[TResult](Expression query, Boolean async, CancellationToken cancellationToken) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\QueryCompiler.cs:line 87
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.Execute[TResult](Expression query) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\QueryCompiler.cs:line 60
   at Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryProvider.Execute[TResult](Expression expression) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\EntityQueryProvider.cs:line 62
   at Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1.GetEnumerator() in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\EntityQueryable`.cs:line 78
   at System.Collections.Generic.List`1..ctor(IEnumerable`1 collection)
   at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
   at SerializeDbContextExtension.FromSerializedExpression[TDbContext](TDbContext dbContext, Func`2 deserializer, String serializedExpression) in C:\github\EfCore.Serialize\src\EfCore.Serialize\Extensions\SerializeDbContextExtension.cs:line 68
   at BlazorApp1.Controllers.DataController.Query(String serializedExpression) in C:\github\EfCore.Serialize\samples\Serialize.Linq\BlazorApp1\Controllers\DataController.cs:line 39
   at lambda_method97(Closure, Object, Object[])
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.SyncObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeActionMethodAsync()
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeNextActionFilterAsync()
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
--- End of stack trace from previous location ---
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
   at Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIMiddleware.Invoke(HttpContext httpContext)
   at Swashbuckle.AspNetCore.Swagger.SwaggerMiddleware.Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

