using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using ExamPrepIdeaCenter.Models;



namespace ExamPrepIdeaCenter

{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string lastCreatedIdeaId;


        private const string BaseUrl = "http://144.91.123.158:82";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI2ZWRkMjQ0ZS0zZDViLTQwODQtOTZiOC1lNmQ1MDQzM2M1OGIiLCJpYXQiOiIwNC8wOC8yMDI2IDE1OjQ2OjM0IiwiVXNlcklkIjoiMDMzOTUzZTMtMzI4YS00NmI4LTUzM2UtMDhkZTc2YTJkM2VjIiwiRW1haWwiOiJ2YW55YUB0ZXN0LmNvbSIsIlVzZXJOYW1lIjoidmFueWF0ZXN0IiwiZXhwIjoxNzc1Njg0Nzk0LCJpc3MiOiJJZGVhQ2VudGVyX0FwcF9Tb2Z0VW5pIiwiYXVkIjoiSWRlYUNlbnRlcl9XZWJBUElfU29mdFVuaSJ9.ztQSgJ1yvjvaycEoZ_50Nbu8_b8n3MwwERDU0jMbh6I";
        private const string LoginEmail = "vanya@test.com";
        private const string LoginPassword = "vanya123";

        [OneTimeSetUp]

        public void Setup()
        {
            string jwtToken;
            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;

            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)

            };
            this.client = new RestClient(options);

        }
        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");

                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to retrieve JWT token. Status code: {response.StatusCode}, Response: {response.Content}");



            }
        }

        [Order(1)]
        [Test]
        public void CreateIdea_WithRequiredFields_ShouldReturnSuccess()
        {
            var ideaData = new IdeaDTO
            {
                Title = "Test Idea Title",
                Description = "Test Idea Description",
                Url = ""

            };

            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaData);

            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));



        }
        [Order(2)]
        [Test]
        public void GetAllIdeas_ShouldReturnSuucess()
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItem = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseItem, Is.Not.Null);
            Assert.That(responseItem, Is.Not.Empty);

        

            lastCreatedIdeaId = responseItem.LastOrDefault()?.Id;


        }
        [Order(3)]
        [Test]
        public void EditExistingIdea_ShouldReturnSuccess()
        {
            var editedIdea = new IdeaDTO
            {
                Title = "Edited Idea Title",
                Description = "This is edited Idea Description",
                Url = ""

            };
            var request = new RestRequest("/api/Idea/Edit", Method.Put);

            request.AddQueryParameter("ideaid", lastCreatedIdeaId);
            request.AddJsonBody(editedIdea);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));


        }
        [Order(4)]
        [Test]
        public void DeleteExistingIdea_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaid", lastCreatedIdeaId);
            var response = this.client.Execute(request);

            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));


        }

        [Order(5)]
        [Test]
        public void CreateIdea_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            var ideaData = new IdeaDTO
            {
                Title = "",
                Description = "Test Idea Description",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaData);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");

        }
        [Order(6)]
        [Test]
        public void EditNonExistingIdea_ShouldReturnBadRequest()
        {
            string nonExistingIdeaId = "999999";
            var editedIdea = new IdeaDTO
            {
                Title = "Edited Idea Title",
                Description = "This is edited Idea Description",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaid", nonExistingIdeaId);
            request.AddJsonBody(editedIdea);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingIdea_ShouldReturnBadRequest()


        {
            string nonExistingIdeaId = "999999";
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaid", nonExistingIdeaId);
            var response = this.client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));


        }

        [OneTimeTearDown]


        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
