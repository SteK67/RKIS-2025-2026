using TodoApp.Models;

namespace TodoList.Tests
{
    public class ProfileTests
    {
        [Fact]
        public void DefaultConstructor_SetsEmptyValues()
        {
            var profile = new Profile();

            Assert.NotEqual(Guid.Empty, profile.Id);
            Assert.Equal(string.Empty, profile.Login);
            Assert.Equal(string.Empty, profile.Password);
            Assert.Equal(string.Empty, profile.FirstName);
            Assert.Equal(string.Empty, profile.LastName);
            Assert.Equal(0, profile.BirthYear);
            Assert.Empty(profile.Todos);
        }

        [Fact]
        public void Constructor_SetsPassedValues()
        {
            var profile = new Profile("ivan", "12345", "Ivan", "Petrov", 2000);

            Assert.NotEqual(Guid.Empty, profile.Id);
            Assert.Equal("ivan", profile.Login);
            Assert.Equal("12345", profile.Password);
            Assert.Equal("Ivan", profile.FirstName);
            Assert.Equal("Petrov", profile.LastName);
            Assert.Equal(2000, profile.BirthYear);
        }

        [Fact]
        public void GetInfo_ReturnsFullNameAndAge()
        {
            int birthYear = DateTime.Now.Year - 20;
            var profile = new Profile("ivan", "12345", "Ivan", "Petrov", birthYear);

            string info = profile.GetInfo();

            Assert.Contains("Ivan Petrov", info);
            Assert.Contains("20", info);
        }
    }
}
