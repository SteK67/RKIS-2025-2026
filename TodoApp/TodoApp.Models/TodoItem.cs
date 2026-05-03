using System;
using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class TodoItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    [Required]
    public TodoStatus Status { get; set; } = TodoStatus.NotStarted;

    [Required]
    public DateTime LastUpdate { get; set; } = DateTime.Now;

    [Required]
    public Guid ProfileId { get; set; }

    public Profile? Profile { get; set; }

    public TodoItem()
    {
    }

    public TodoItem(string text, Guid profileId)
    {
        Text = text;
        ProfileId = profileId;
        Status = TodoStatus.NotStarted;
        LastUpdate = DateTime.Now;
    }

    public void UpdateText(string text)
    {
        Text = text;
        LastUpdate = DateTime.Now;
    }

    public void SetStatus(TodoStatus status)
    {
        Status = status;
        LastUpdate = DateTime.Now;
    }
}
