using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StandForFH5Revival
{
    public partial class Form1 : Form
    {
        private bool _isGameConnected = false;
        private MemoryManager _memoryManager;

        public Form1()
        {
            InitializeComponent();
            _memoryManager = new MemoryManager();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateStatus("Please wait...");
            ProcessInit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                CleanupResources();
            }
            catch { }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            var lpsize = flowLayoutPanel1.Size;
            lpsize.Width = Size.Width - 40;
            lpsize.Height = Size.Height - 76;
            flowLayoutPanel1.Size = lpsize;
        }

        private void ProcessInit()
        {
            if (_memoryManager.IsGameRunning())
            {
                ProcessFound();
            }
            else
            {
                UpdateStatus("Please open Forza Horizon 5.");
                processWaitTimer.Start();
            }
        }

        private void processWaitTimer_Tick(object sender, EventArgs e)
        {
            if (_memoryManager.IsGameRunning())
            {
                processWaitTimer.Stop();
                processStartTimer.Start();
            }
        }

        private void processStartTimer_Tick(object sender, EventArgs e)
        {
            processStartTimer.Stop();
            ProcessFound();
        }

        private void ProcessFound()
        {
            UpdateStatus("Connecting to game process...");
            
            try
            {
                if (_memoryManager.AttachToGame())
                {
                    UpdateStatus("Scanning patterns...");
                    patternscanTimer.Start();
                }
                else
                {
                    UpdateStatus("Failed to attach to game process. Check administrator privileges.");
                    processWaitTimer.Start();
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to connect to game: {ex.Message}");
                ProcessInit();
            }
        }

        private void patternscanTimer_Tick(object sender, EventArgs e)
        {
            patternscanTimer.Stop();
            UpdateStatus("Ready.");
            _isGameConnected = true;
            EnableControls(true);
            mainloopTimer.Start();
        }

        private void mainloopTimer_Tick(object sender, EventArgs e)
        {
            if (!_memoryManager.IsAttached)
            {
                mainloopTimer.Stop();
                _isGameConnected = false;
                EnableControls(false);
                UpdateStatus("Game connection lost. Waiting for game...");
                ProcessInit();
            }
        }

        private async void addCreditsBtn_Click(object sender, EventArgs e)
        {
            if (!_isGameConnected)
            {
                UpdateStatus("Game not connected.");
                return;
            }

            if (creditsVal.Value <= 0)
            {
                UpdateStatus("Please enter a positive amount.");
                return;
            }

            try
            {
                var amount = (float)creditsVal.Value;
                UpdateStatus($"Adding {amount:N0} credits...");
                addCreditsBtn.Enabled = false;
                
                var unlocksCheats = Cheats.GetClass<UnlocksCheats>();
                bool success = await unlocksCheats.AddCredits(amount);
                
                if (success)
                {
                    UpdateStatus($"Successfully added {amount:N0} credits!");
                }
                else
                {
                    UpdateStatus("Failed to add credits. Check game connection.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error adding credits: {ex.Message}");
            }
            finally
            {
                addCreditsBtn.Enabled = _isGameConnected;
            }
        }

        private async void addWheelspinsBtn_Click(object sender, EventArgs e)
        {
            if (!_isGameConnected)
            {
                UpdateStatus("Game not connected.");
                return;
            }

            if (wheelspinsVal.Value <= 0)
            {
                UpdateStatus("Please enter a positive amount of Wheelspins.");
                return;
            }

            try
            {
                var amount = (float)wheelspinsVal.Value;
                UpdateStatus($"Adding {amount:N0} Wheelspins...");
                addWheelspinsBtn.Enabled = false;

                var unlocksCheats = Cheats.GetClass<UnlocksCheats>();
                bool success = await unlocksCheats.CheatWheelspins(amount);

                if (success)
                {
                    UpdateStatus($"Successfully added {amount:N0} Wheelspins!");
                }
                else
                {
                    UpdateStatus("Failed to add Wheelspins. Check game connection.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error adding Wheelspins: {ex.Message}");
            }
            finally
            {
                addWheelspinsBtn.Enabled = _isGameConnected;
            }
        }

        private async void addSuperWheelspinsBtn_Click(object sender, EventArgs e)
        {
            if (!_isGameConnected)
            {
                UpdateStatus("Game not connected.");
                return;
            }

            if (superWheelspinsVal.Value <= 0)
            {
                UpdateStatus("Please enter a positive amount of Super Wheelspins.");
                return;
            }

            try
            {
                var amount = (float)superWheelspinsVal.Value;
                UpdateStatus($"Adding {amount:N0} Super Wheelspins...");
                addSuperWheelspinsBtn.Enabled = false;

                var unlocksCheats = Cheats.GetClass<UnlocksCheats>();
                bool success = await unlocksCheats.CheatSuperWheelspins(amount);

                if (success)
                {
                    UpdateStatus($"Successfully added {amount:N0} Super Wheelspins!");
                }
                else
                {
                    UpdateStatus("Failed to add Super Wheelspins. Check game connection.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error adding Super Wheelspins: {ex.Message}");
            }
            finally
            {
                addSuperWheelspinsBtn.Enabled = _isGameConnected;
            }
        }

        private async void addXpBtn_Click(object sender, EventArgs e)
        {
            if (!_isGameConnected)
            {
                UpdateStatus("Game not connected.");
                return;
            }

            if (xpVal.Value <= 0)
            {
                UpdateStatus("Please enter a positive amount of XP.");
                return;
            }

            try
            {
                var amount = (float)xpVal.Value;
                UpdateStatus($"Adding {amount:N0} XP...");
                addXpBtn.Enabled = false;
                
                var unlocksCheats = Cheats.GetClass<UnlocksCheats>();
                bool success = await unlocksCheats.CheatXP(amount);
                
                if (success)
                {
                    UpdateStatus($"Successfully added {amount:N0} XP!");
                }
                else
                {
                    UpdateStatus("Failed to add XP. Check game connection.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error adding XP: {ex.Message}");
            }
            finally
            {
                addXpBtn.Enabled = _isGameConnected;
            }
        }

        private async void autoshowAllfree_Click(object sender, EventArgs e)
        {
            if (!_isGameConnected)
            {
                UpdateStatus("Game not connected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(carIdsTextBox.Text))
            {
                UpdateStatus("Please enter car IDs (comma separated).");
                return;
            }

            try
            {
                UpdateStatus("Adding cars to garage...");
                autoshowAllfree.Enabled = false;

                var autoshowCheats = Cheats.GetClass<AutoshowCheats>();
                bool success = await autoshowCheats.AddCustomCars(carIdsTextBox.Text);

                if (success)
                {
                    UpdateStatus("Successfully added cars to garage!");
                }
                else
                {
                    UpdateStatus("Failed to add cars. Check game connection or car IDs.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error adding cars: {ex.Message}");
            }
            finally
            {
                autoshowAllfree.Enabled = _isGameConnected;
            }
        }

        private async void addAllCarsBtn_Click(object sender, EventArgs e)
        {
            if (!_isGameConnected)
            {
                UpdateStatus("Game not connected.");
                return;
            }

            try
            {
                UpdateStatus("Adding all cars to garage...");
                addAllCarsBtn.Enabled = false;

                var autoshowCheats = Cheats.GetClass<AutoshowCheats>();
                bool success = await autoshowCheats.AddAllCars();

                if (success)
                {
                    UpdateStatus("Successfully added all cars to garage!");
                }
                else
                {
                    UpdateStatus("Failed to add all cars. Check game connection.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error adding all cars: {ex.Message}");
            }
            finally
            {
                addAllCarsBtn.Enabled = _isGameConnected;
            }
        }

        private async void showAllCarsBtn_Click(object sender, EventArgs e)
        {
            if (!_isGameConnected)
            {
                UpdateStatus("Game not connected.");
                return;
            }

            try
            {
                UpdateStatus("Showing rare cars in Autoshow...");
                showAllCarsBtn.Enabled = false;

                var autoshowCheats = Cheats.GetClass<AutoshowCheats>();
                bool success = await autoshowCheats.ShowOnlyRareCarsInAutoshow();

                if (success)
                {
                    UpdateStatus("Successfully unlocked rare cars in Autoshow!");
                }
                else
                {
                    UpdateStatus("Failed to show rare cars. Check game connection.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error showing rare cars: {ex.Message}");
            }
            finally
            {
                showAllCarsBtn.Enabled = _isGameConnected;
            }
        }

        private async void makeCarsFreeBtn_Click(object sender, EventArgs e)
        {
            if (!_isGameConnected)
            {
                UpdateStatus("Game not connected.");
                return;
            }

            try
            {
                UpdateStatus("Making all cars free in Autoshow...");
                makeCarsFreeBtn.Enabled = false;

                var autoshowCheats = Cheats.GetClass<AutoshowCheats>();
                bool success = await autoshowCheats.MakeAllCarsFree();

                if (success)
                {
                    UpdateStatus("Successfully made all cars free!");
                }
                else
                {
                    UpdateStatus("Failed to make cars free. Check game connection.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error making cars free: {ex.Message}");
            }
            finally
            {
                makeCarsFreeBtn.Enabled = _isGameConnected;
            }
        }

        private void autoshowAvailableBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isGameConnected) return;

            try
            {
                UpdateStatus($"Autoshow setting: {autoshowAvailableBox.Text}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            if (toolStripStatusLabel1 != null)
            {
                toolStripStatusLabel1.Text = message;
            }
        }

        private void EnableControls(bool enabled)
        {
            addCreditsBtn.Enabled = enabled;
            creditsVal.Enabled = enabled;
            
            addWheelspinsBtn.Enabled = enabled;
            wheelspinsVal.Enabled = enabled;
            
            addSuperWheelspinsBtn.Enabled = enabled;
            superWheelspinsVal.Enabled = enabled;
            
            addXpBtn.Enabled = enabled;
            xpVal.Enabled = enabled;
            
            carIdsTextBox.Enabled = enabled;
            autoshowAllfree.Enabled = enabled;
            addAllCarsBtn.Enabled = enabled;
            showAllCarsBtn.Enabled = enabled;
            makeCarsFreeBtn.Enabled = enabled;
            autoshowAvailableBox.Enabled = enabled;
        }

        private void CleanupResources()
        {
            try
            {
                _isGameConnected = false;
                StopAllTimers();
                CleanupMemoryManager();
                CleanupCheatInstances();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch { }
        }

        private void StopAllTimers()
        {
            try
            {
                processWaitTimer?.Stop();
                patternscanTimer?.Stop();
                mainloopTimer?.Stop();
                processStartTimer?.Stop();
            }
            catch { }
        }

        private void CleanupMemoryManager()
        {
            try
            {
                if (_memoryManager != null)
                {
                    _memoryManager.Dispose();
                    _memoryManager = null;
                }
            }
            catch { }
        }

        private void CleanupCheatInstances()
        {
            try
            {
                if (Cheats.IsInstanceCached<UnlocksCheats>())
                {
                    Cheats.GetClass<UnlocksCheats>()?.Cleanup();
                }

                if (Cheats.IsInstanceCached<Sql>())
                {
                    Cheats.GetClass<Sql>()?.Cleanup();
                }
                
                if (Cheats.IsInstanceCached<AutoshowCheats>())
                {
                    Cheats.GetClass<AutoshowCheats>()?.Cleanup();
                }

                if (Cheats.IsInstanceCached<Bypass>())
                {
                    Cheats.GetClass<Bypass>()?.Cleanup();
                }
                
                Cheats.ClearCache();
            }
            catch { }
        }
    }
}