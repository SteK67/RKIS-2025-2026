using TodoApp.Commands;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;
using InvalidArgumentException = TodoApp.Exceptions.InvalidArgumentException;

namespace TodoList.Tests
{
    public class CommandParserTests
    {
        [Theory]
        [InlineData("help", typeof(HelpCommand))]
        [InlineData("profile", typeof(ProfileCommand))]
        [InlineData("profile -o", typeof(ProfileCommand))]
        [InlineData("add \"Buy milk\"", typeof(AddCommand))]
        [InlineData("add -m", typeof(AddCommand))]
        [InlineData("view", typeof(ViewCommand))]
        [InlineData("view -a", typeof(ViewCommand))]
        [InlineData("read 0", typeof(ReadCommand))]
        [InlineData("status 0 completed", typeof(StatusCommand))]
        [InlineData("update 0 \"New text\"", typeof(UpdateCommand))]
        [InlineData("delete 0", typeof(DeleteCommand))]
        [InlineData("search --contains \"milk\"", typeof(SearchCommand))]
        [InlineData("load 1 10", typeof(LoadCommand))]
        [InlineData("sync --push", typeof(SyncCommand))]
        [InlineData("undo", typeof(UndoCommand))]
        [InlineData("redo", typeof(RedoCommand))]
        public void Parse_WithValidCommand_ReturnsExpectedCommandType(string input, Type expectedType)
        {
            var command = CommandParser.Parse(input);

            Assert.IsType(expectedType, command);
        }

        [Fact]
        public void Parse_WithUnknownCommand_ThrowsInvalidCommandException()
        {
            Assert.Throws<InvalidCommandException>(() => CommandParser.Parse("unknown"));
        }

        [Theory]
        [InlineData("read")]
        [InlineData("read abc")]
        [InlineData("delete")]
        [InlineData("delete abc")]
        [InlineData("status 0 wrong")]
        [InlineData("update 0 \"\"")]
        [InlineData("load 0 10")]
        [InlineData("load abc 10")]
        [InlineData("sync")]
        public void Parse_WithInvalidArguments_ThrowsInvalidArgumentException(string input)
        {
            Assert.Throws<InvalidArgumentException>(() => CommandParser.Parse(input));
        }

        [Theory]
        [InlineData("notstarted", TodoStatus.NotStarted)]
        [InlineData("not-started", TodoStatus.NotStarted)]
        [InlineData("inprogress", TodoStatus.InProgress)]
        [InlineData("in-progress", TodoStatus.InProgress)]
        [InlineData("completed", TodoStatus.Completed)]
        [InlineData("postponed", TodoStatus.Postponed)]
        [InlineData("failed", TodoStatus.Failed)]
        public void TryParseStatus_WithValidStatus_ReturnsTrue(string value, TodoStatus expectedStatus)
        {
            bool result = CommandParser.TryParseStatus(value, out var status);

            Assert.True(result);
            Assert.Equal(expectedStatus, status);
        }

        [Fact]
        public void TryParseStatus_WithInvalidStatus_ReturnsFalse()
        {
            bool result = CommandParser.TryParseStatus("wrong", out _);

            Assert.False(result);
        }
    }
}
