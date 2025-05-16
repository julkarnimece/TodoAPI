using System.Collections.Concurrent;
using Todo.API.Models;

namespace Todo.API.InMemoryDataStore
{
    public class TodoStore
    {
        private readonly ConcurrentDictionary<Guid, Models.TodoItem> _todos = new();

        public IEnumerable<Models.TodoItem> GetAll() => _todos.Values;

        public Models.TodoItem? Get(Guid id) => _todos.TryGetValue(id, out var item) ? item : null;

        public Models.TodoItem Create(string text)
        {
            var item = new Models.TodoItem(Guid.NewGuid(), text, false, DateTime.UtcNow);
            _todos[item.Id] = item;
            return item;
        }

        public bool Update(Guid id, string text, bool isComplete)
        {
            if (!_todos.TryGetValue(id, out var existing)) return false;

            var updated = existing with { Text = text, IsComplete = isComplete };
            _todos[id] = updated;
            return true;
        }

        public bool Delete(Guid id) => _todos.TryRemove(id, out _);
    }
}
