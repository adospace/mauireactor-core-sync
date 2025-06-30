using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Data;

[Table("TodoItems")]
public class TodoItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public bool IsCompleted { get; set; }

    public override string ToString()
    {
        return $"{Id}: {Title} - {(IsCompleted ? "Completed" : "Pending")}";
    }
}
