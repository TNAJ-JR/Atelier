using Atelier.Enum;

namespace Atelier.Modeles
{
    public static class OrderWorkflow
    {
        private static readonly IReadOnlyDictionary<OrderStatus, IReadOnlyList<OrderStatus>> Allowed =
            new Dictionary<OrderStatus, IReadOnlyList<OrderStatus>>
            {
                [OrderStatus.Draft] = new[] { OrderStatus.Confirmed, OrderStatus.Cancelled },
                [OrderStatus.Confirmed] = new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
                [OrderStatus.Shipped] = Array.Empty<OrderStatus>(),
                [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
            };

        public static bool CanTransition(OrderStatus from, OrderStatus to)
            => Allowed[from].Contains(to);

        public static IReadOnlyList<OrderStatus> NextStates(OrderStatus from)
            => Allowed[from];

        /// <summary>Draft -> Confirmed : décrémente le stock.</summary>
        public static bool DecrementsStock(OrderStatus from, OrderStatus to)
            => from == OrderStatus.Draft && to == OrderStatus.Confirmed;

        /// <summary>Confirmed -> Cancelled : restaure le stock. Un draft annulé n'a rien réservé.</summary>
        public static bool RestoresStock(OrderStatus from, OrderStatus to)
            => from == OrderStatus.Confirmed && to == OrderStatus.Cancelled;
    }
}
