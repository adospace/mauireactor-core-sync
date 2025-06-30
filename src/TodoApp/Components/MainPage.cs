using CoreSync;
using CoreSync.Http.Client;
using MauiReactor;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Data;

namespace TodoApp.Components;

class MainPageState
{
    public bool IsBusy { get; set; } = false;

    public ObservableCollection<TodoItem> TodoItems { get; set; } = [];

    public string NewTodoItemTitle { get; set; } = string.Empty;
}

class MainPage : Component<MainPageState>
{
    public override VisualNode Render()
    {
        return ContentPage(

            State.IsBusy ? ActivityIndicator().IsRunning(true).IsVisible(true) :
            Grid("Auto,*,Auto","*",

                Grid("*","*,Auto",
                    Entry()
                        .Text(State.NewTodoItemTitle)
                        .OnTextChanged(newText => SetState(s => s.NewTodoItemTitle = newText, false)),

                    Button("Add")
                        .OnClicked(CreateTodoItem)
                        .IsEnabled(()=>!State.IsBusy && !string.IsNullOrWhiteSpace(State.NewTodoItemTitle) && State.NewTodoItemTitle.Length > 3)
                        .GridColumn(1)

                )
                .ColumnSpacing(10),
            
                CollectionView()
                    .ItemsSource(State.TodoItems, item =>
                        Grid("*","Auto, *",
                            CheckBox()
                                .IsChecked(item.IsCompleted)
                                .OnCheckedChanged(completed => SetItemState(item, completed)),

                            Label(item.Title)
                                .TextDecorations(item.IsCompleted ? TextDecorations.Strikethrough : TextDecorations.None)
                                .VCenter()
                                .GridColumn(1)
                        )
                        .ColumnSpacing(10)        
                    )
                    .GridRow(1),

                Button("Synchronize")
                    .OnClicked(SyncWithServer)
                    .GridRow(2)
            )
            .RowSpacing(10)
        )
        .Padding(5)
        .OnAppearing(SetupDb)
        .Title("Todo App");
    }

    async Task CreateTodoItem()
    {
        using var serviceScope = Services.CreateScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var newItem = new TodoItem { Title = State.NewTodoItemTitle };

        db.TodoItems.Add(newItem);

        await db.SaveChangesAsync();

        SetState(s =>
        {
            s.TodoItems.Add(newItem);
            s.NewTodoItemTitle = string.Empty;
        });
    }

    async Task SetItemState(TodoItem item, bool completed)
    {
        using var serviceScope = Services.CreateScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        item.IsCompleted = completed;

        db.TodoItems.Update(item);

        await db.SaveChangesAsync();

        SetState(s =>
        {
            var index = s.TodoItems.IndexOf(item);
            if (index >= 0)
            {
                s.TodoItems[index] = item; // Update the item in the collection
            }
        });
    }

    async Task SetupDb()
    {
        SetState(s => s.IsBusy = true);
        try
        {
            using var serviceScope = Services.CreateScope();
            var db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            var syncProvider = Services.GetRequiredService<ISyncProvider>();

            await syncProvider.ApplyProvisionAsync();

            var items = await db.TodoItems.ToListAsync();

            SetState(s => s.TodoItems = new ObservableCollection<TodoItem>(items));
        }
        finally
        {
            SetState(s => s.IsBusy = false);
        }
    }

    async Task SyncWithServer()
    {
        SetState(s => s.IsBusy = true);
        try
        {
            var localSyncProvider = Services.GetRequiredService<ISyncProvider>();
            var remoteSyncProvider = Services.GetRequiredService<ISyncProviderHttpClient>();

            await localSyncProvider.ApplyProvisionAsync();

            var syncAgent = new SyncAgent(localSyncProvider, remoteSyncProvider);

            await syncAgent.SynchronizeAsync(conflictResolutionOnLocalStore: ConflictResolution.ForceWrite);

            using var serviceScope = Services.CreateScope();
            var db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var items = await db.TodoItems.ToListAsync();

            SetState(s => s.TodoItems = new ObservableCollection<TodoItem>(items));
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            SetState(s => s.IsBusy = false);
        }
    }
}
