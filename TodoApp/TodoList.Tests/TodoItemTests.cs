using TodoApp.Models;

namespace TodoList.Tests
{
    public class TodoItemTests
    {
        [Fact]
        public void DefaultConstructor_SetsDefaultValues()
        {
            var item = new TodoItem();

            Assert.Equal(string.Empty, item.Text);
            Assert.Equal(TodoStatus.NotStarted, item.Status);
            Assert.True(item.LastUpdate <= DateTime.Now);
        }

        [Fact]
        public void Constructor_SetsTextAndDefaultStatus()
        {
            var item = new TodoItem("Buy milk");

            Assert.Equal("Buy milk", item.Text);
            Assert.Equal(TodoStatus.NotStarted, item.Status);
            Assert.True(item.LastUpdate <= DateTime.Now);
        }

        [Fact]
        public void UpdateText_ChangesTextAndLastUpdate()
        {
            var item = new TodoItem("Old text");
            var oldDate = item.LastUpdate;

            item.UpdateText("New text");

            Assert.Equal("New text", item.Text);
            Assert.True(item.LastUpdate >= oldDate);
        }

        [Fact]
        public void SetStatus_ChangesStatusAndLastUpdate()
        {
            var item = new TodoItem("Task");
            var oldDate = item.LastUpdate;

            item.SetStatus(TodoStatus.Completed);

            Assert.Equal(TodoStatus.Completed, item.Status);
            Assert.True(item.LastUpdate >= oldDate);
        }

        [Fact]
        public void GetShortInfo_TruncatesLongText()
        {
            var item = new TodoItem("12345678901234567890123456789012345");

            string shortInfo = item.GetShortInfo();

            Assert.EndsWith("...", shortInfo);
            Assert.True(shortInfo.Length <= 33);
        }

        [Fact]
        public void GetFullInfo_ContainsTextStatusAndDate()
        {
            var item = new TodoItem("Task");
            item.SetStatus(TodoStatus.InProgress);

            string fullInfo = item.GetFullInfo();

            Assert.Contains("Task", fullInfo);
            Assert.Contains("InProgress", fullInfo);
            Assert.Contains(item.LastUpdate.ToString("yyyy-MM-dd"), fullInfo);
        }
    }
}
