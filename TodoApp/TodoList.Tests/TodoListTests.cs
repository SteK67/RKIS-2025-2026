using TodoApp.Models;

namespace TodoList.Tests
{
    public class TodoListTests
    {
        [Fact]
        public void Add_IncreasesCountAndRaisesEvent()
        {
            var list = new TodoApp.Models.TodoList();
            var item = new TodoItem("Task");
            bool eventRaised = false;

            list.OnTodoAdded += addedItem => eventRaised = addedItem == item;
            list.Add(item);

            Assert.True(eventRaised);
            Assert.Equal(1, list.Count);
            Assert.Same(item, list[0]);
        }

        [Fact]
        public void Delete_RemovesItemAndRaisesEvent()
        {
            var list = new TodoApp.Models.TodoList();
            var item = new TodoItem("Task");
            bool eventRaised = false;
            list.Add(item);

            list.OnTodoDeleted += deletedItem => eventRaised = deletedItem == item;
            list.Delete(0);

            Assert.True(eventRaised);
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void Delete_WithInvalidIndex_DoesNotChangeList()
        {
            var list = new TodoApp.Models.TodoList();
            var item = new TodoItem("Task");
            list.Add(item);

            list.Delete(5);

            Assert.Equal(1, list.Count);
            Assert.Same(item, list[0]);
        }

        [Fact]
        public void GetItem_WithInvalidIndex_ReturnsNull()
        {
            var list = new TodoApp.Models.TodoList();

            var item = list.GetItem(0);

            Assert.Null(item);
        }

        [Fact]
        public void SetStatus_ChangesItemStatusAndRaisesEvent()
        {
            var list = new TodoApp.Models.TodoList();
            var item = new TodoItem("Task");
            bool eventRaised = false;
            list.Add(item);

            list.OnStatusChanged += changedItem => eventRaised = changedItem == item;
            list.SetStatus(0, TodoStatus.Completed);

            Assert.True(eventRaised);
            Assert.Equal(TodoStatus.Completed, list[0].Status);
        }

        [Fact]
        public void UpdateItem_ChangesTextAndRaisesEvent()
        {
            var list = new TodoApp.Models.TodoList();
            var item = new TodoItem("Task");
            bool eventRaised = false;
            list.Add(item);

            list.OnTodoUpdated += updatedItem => eventRaised = updatedItem == item;
            list.UpdateItem(0, "Updated task");

            Assert.True(eventRaised);
            Assert.Equal("Updated task", list[0].Text);
        }

        [Fact]
        public void IndexerSetter_ReplacesItemAndRaisesEvent()
        {
            var list = new TodoApp.Models.TodoList();
            var firstItem = new TodoItem("First");
            var secondItem = new TodoItem("Second");
            bool eventRaised = false;
            list.Add(firstItem);

            list.OnTodoUpdated += updatedItem => eventRaised = updatedItem == secondItem;
            list[0] = secondItem;

            Assert.True(eventRaised);
            Assert.Same(secondItem, list[0]);
        }

        [Fact]
        public void GetAll_ReturnsAllItems()
        {
            var list = new TodoApp.Models.TodoList();
            list.Add(new TodoItem("First"));
            list.Add(new TodoItem("Second"));

            var items = list.GetAll();

            Assert.Equal(2, items.Count);
            Assert.Equal("First", items[0].Text);
            Assert.Equal("Second", items[1].Text);
        }
    }
}
