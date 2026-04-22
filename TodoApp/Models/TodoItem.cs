using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TodoApp.Services;

namespace TodoApp.Models
{
    public class TodoItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Text { get; set; }

        [Required]
        public TodoStatus Status { get; set; }

        [Required]
        public DateTime LastUpdate { get; set; }

        [Required]
        public Guid ProfileId { get; set; }

        [ForeignKey(nameof(ProfileId))]
        public Profile? Profile { get; set; }

        [NotMapped]
        private readonly IClock _clock;

        public TodoItem()
            : this(string.Empty, new SystemClock())
        {
        }

        public TodoItem(string text)
            : this(text, new SystemClock())
        {
        }

        public TodoItem(string text, IClock clock)
        {
            _clock = clock;
            Text = text;
            Status = TodoStatus.NotStarted;
            LastUpdate = _clock.Now;
        }

        public void UpdateText(string newText)
        {
            Text = newText;
            LastUpdate = _clock.Now;
        }

        public void SetStatus(TodoStatus status)
        {
            Status = status;
            LastUpdate = _clock.Now;
        }

        public string GetShortInfo()
        {
			string shortText = Text.Length > 30 
                ? Text.Replace("\n", " ").Substring(0, 30) + "..." 
                : Text;
            return shortText;
        }

        public string GetFullInfo()
        {
            return $"Текст: {Text}\nСтатус: {Status}\nПоследнее изменение: {LastUpdate:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
