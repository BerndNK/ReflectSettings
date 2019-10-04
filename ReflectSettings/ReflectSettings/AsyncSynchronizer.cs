using System;
using System.Linq;
using System.Threading.Tasks;

namespace ReflectSettings
{
    class AsyncSynchronizer
    {
        public bool IsBusy { get; set; }

        private Task _currentTask;

        public void UpdateAll(ChangeTrackingManager changeTrackingManager)
        {
            WaitForCurrentTask();

            _currentTask = UpdateAllIntern(changeTrackingManager);

        }

        private void WaitForCurrentTask()
        {
            _currentTask?.Wait();
            _currentTask = null;
        }


        private async Task UpdateAllIntern(ChangeTrackingManager changeTrackingManager)
        {
            IsBusy = true;
            var editables = changeTrackingManager.ToList();

            try
            {
                foreach (var editable in editables)
                {
                    await editable.UpdateCalculatedValuesAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            IsBusy = false;
        }

    }
}
