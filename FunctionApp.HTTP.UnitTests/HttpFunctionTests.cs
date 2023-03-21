using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

namespace FunctionApp.HTTP.UnitTests
{
    public class HttpFunctionTests
    {
        [Fact]
        public async void HttpExampleFunctionSuccess()
        {
            //Arrange
            var queryStringValue = "abc";
            var person = new Person("Alex", 38);
            var context = new DefaultHttpContext();
            var request = new DefaultHttpRequest(context)
            {
                Query = new QueryCollection(

                    new Dictionary<string, StringValues>()
                    {
                        { "name", queryStringValue }
                    }
                ),

                Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(person)))
            };

            var logger = NullLoggerFactory.Instance.CreateLogger("Null logger");

            //Act
            var response = await HttpExampleFunctions.Run(request, logger);

            //Assert
            response.Should().BeAssignableTo<OkObjectResult>();
            var result = (OkObjectResult)response;
            result.Value.Should().BeAssignableTo<string>();
            result.Value.ToString().Should().StartWith($"Hello, {person.Name}. Your age is {person.Age}");

        }
    }
}