using golden_fork.Front.DTOs.Kitchen;

namespace golden_fork.Front.Services
{
    public class CartService
    {
        public event Action OnChange;
        private void NotifyChange() => OnChange?.Invoke();


        private List<CartItem> _items = new();

        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

        public void AddItem(ItemResponse item, int quantity = 1)
        {
            var existing = _items.FirstOrDefault(x => x.Item.Id == item.Id);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _items.Add(new CartItem { Item = item, Quantity = quantity });
            }
            NotifyChange();
        }

        public void UpdateQuantity(int itemId, int quantity)
        {
            var item = _items.FirstOrDefault(x => x.Item.Id == itemId);
            if (item != null)
            {
                if (quantity <= 0)
                    _items.Remove(item);
                else
                    item.Quantity = quantity;
                NotifyChange();
            }
        }

        public void RemoveItem(int itemId)
        {
            _items.RemoveAll(x => x.Item.Id == itemId);
            NotifyChange();
        }

        public void Clear()
        {
            _items.Clear();
            NotifyChange();
        }

        public decimal GetTotalPrice()
        {
            return _items.Sum(x => x.GetPrice() * x.Quantity);
        }

        public int GetTotalItemsCount()
        {
            return _items.Sum(x => x.Quantity);
        }

    }

    public class CartItem
    {
        public ItemResponse Item { get; set; } = new();
        public int Quantity { get; set; } = 1;

        public decimal GetPrice() => Item.SpecialPrice ?? Item.Price;
    }
}