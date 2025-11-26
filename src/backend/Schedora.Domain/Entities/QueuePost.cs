using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class QueuePost : Entity
    {
        // Properties
        public long QueueId { get; private set; }
        public long PostId { get; private set; }
        public int OrderIndex { get; private set; }
        public DateTime AddedAt { get; private set; }

        // Navigation Properties
        public virtual Queue Queue { get; private set; }
        public virtual Post Post { get; private set; }

        // Private constructor for EF
        private QueuePost() { }

        // Factory method
        public static QueuePost Create(long queueId, long postId, int orderIndex)
        {
            return new QueuePost
            {
                QueueId = queueId,
                PostId = postId,
                OrderIndex = orderIndex,
                AddedAt = DateTime.UtcNow
            };
        }

        // Domain methods
        public void UpdateOrder(int newOrder)
        {
            OrderIndex = newOrder;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
