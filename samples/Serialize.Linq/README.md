Current issue

System.InvalidOperationException: The LINQ expression 'NoNameParameter
    .Where(wf => wf.TemperatureC > -999)' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
   at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\NavigationExpandingExpressionVisitor.cs:line 803
   at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\NavigationExpandingExpressionVisitor.cs:line 328
   at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\NavigationExpandingExpressionVisitor.cs:line 328
   at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.VisitMethodCall(MethodCallExpression methodCallExpression) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\NavigationExpandingExpressionVisitor.cs:line 328
   at Microsoft.EntityFrameworkCore.Query.Internal.NavigationExpandingExpressionVisitor.Expand(Expression query) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\NavigationExpandingExpressionVisitor.cs:line 101
   at Microsoft.EntityFrameworkCore.Query.QueryTranslationPreprocessor.Process(Expression query) in C:\github\EfCore.Serialize\src\EFCore\Query\QueryTranslationPreprocessor.cs:line 59
   at Microsoft.EntityFrameworkCore.InMemory.Query.Internal.InMemoryQueryTranslationPreprocessor.Process(Expression query)
   at Microsoft.EntityFrameworkCore.Query.QueryCompilationContext.CreateQueryExecutorExpression[TResult](Expression query) in C:\github\EfCore.Serialize\src\EFCore\Query\QueryCompilationContext.cs:line 256
   at Microsoft.EntityFrameworkCore.Query.QueryCompilationContext.CreateQueryExecutor[TResult](Expression query) in C:\github\EfCore.Serialize\src\EFCore\Query\QueryCompilationContext.cs:line 224
   at Microsoft.EntityFrameworkCore.Storage.Database.CompileQuery[TResult](Expression query, Boolean async) in C:\github\EfCore.Serialize\src\EFCore\Storage\Database.cs:line 66
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.CompileQueryCore[TResult](IDatabase database, Expression query, IModel model, Boolean async) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\QueryCompiler.cs:line 127
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.<>c__DisplayClass11_0`1.<ExecuteCore>b__0() in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\QueryCompiler.cs:line 83
   at Microsoft.EntityFrameworkCore.Query.Internal.CompiledQueryCache.GetOrAddQuery[TResult](Object cacheKey, Func`1 compiler) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\CompiledQueryCache.cs:line 64
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.ExecuteCore[TResult](Expression query, Boolean async, CancellationToken cancellationToken) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\QueryCompiler.cs:line 79
   at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.Execute[TResult](Expression query) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\QueryCompiler.cs:line 60
   at Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryProvider.Execute[TResult](Expression expression) in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\EntityQueryProvider.cs:line 62
   at Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1.GetEnumerator() in C:\github\EfCore.Serialize\src\EFCore\Query\Internal\EntityQueryable`.cs:line 78
   at System.Collections.Generic.List`1..ctor(IEnumerable`1 collection)
   at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
   at SerializeDbContextExtension.FromSerializedExpression[TDbContext](TDbContext dbContext, Func`2 deserializer, String serializedExpression) in C:\github\EfCore.Serialize\src\EfCore.Serialize\Extensions\SerializeDbContextExtension.cs:line 65
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
