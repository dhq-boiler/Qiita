﻿using boilersGraphics.Dao;
using boilersGraphics.Dao.Migration.Plan;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Views;
using Homura.Core;
using Homura.ORM;
using Homura.ORM.Setup;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private DiagramViewModel _DiagramViewModel;
        private ToolBarViewModel _ToolBarViewModel;
        private CompositeDisposable _CompositeDisposable = new CompositeDisposable();
        private IDialogService dlgService = null;
        private DateTime _StartUpTime;

        public static MainWindowViewModel Instance { get; set; }

        public MainWindowViewModel()
        {
            Instance = this;
        }

        public MainWindowViewModel(IDialogService dialogService)
        {
            Instance = this;
            this.dlgService = dialogService;

            ConfigureNLog();

            if (App.IsTest)
            {
                ConnectionManager.SetDefaultConnection($"DataSource=bg.db", typeof(SQLiteConnection));
            }
            else
            {
                var dbDirectory = System.IO.Path.Combine(boilersGraphics.Helpers.Path.GetRoamingDirectory(), @"dhq_boiler\boilersGraphics");
                var dbFilePath = System.IO.Path.Combine(dbDirectory, "bg.db");
                Directory.CreateDirectory(dbDirectory);
                ConnectionManager.SetDefaultConnection($"DataSource={dbFilePath}", typeof(SQLiteConnection));
            }

            ManagebGDB();

            _StartUpTime = DateTime.Now;

            Recorder = new OperationRecorder(Controller);

            DiagramViewModel = new DiagramViewModel(this, this.dlgService, 1000, 1000);
            _CompositeDisposable.Add(DiagramViewModel);
            DiagramViewModel.EnableMiniMap.Value = true;
            ToolBarViewModel = new ToolBarViewModel(dialogService, this);

            DiagramViewModel.EnableMiniMap.Value = true;
            DiagramViewModel.EnableBrushThickness.Value = true;

            Title.Value = $"{App.GetAppNameAndVersion()}";
            DiagramViewModel.FileName.Value = "*";

            EdgeThicknessOptions.Add(double.NaN);
            EdgeThicknessOptions.Add(0.0);
            EdgeThicknessOptions.Add(1.0);
            EdgeThicknessOptions.Add(2.0);
            EdgeThicknessOptions.Add(3.0);
            EdgeThicknessOptions.Add(4.0);
            EdgeThicknessOptions.Add(5.0);
            EdgeThicknessOptions.Add(10.0);
            EdgeThicknessOptions.Add(15.0);
            EdgeThicknessOptions.Add(20.0);
            EdgeThicknessOptions.Add(25.0);
            EdgeThicknessOptions.Add(30.0);
            EdgeThicknessOptions.Add(35.0);
            EdgeThicknessOptions.Add(40.0);
            EdgeThicknessOptions.Add(45.0);
            EdgeThicknessOptions.Add(50.0);
            EdgeThicknessOptions.Add(100.0);

            DeleteSelectedItemsCommand = new DelegateCommand<object>(p =>
            {
                ExecuteDeleteSelectedItemsCommand(p);
            });
            ExitApplicationCommand = new DelegateCommand(() =>
            {
                Application.Current.Shutdown();
            });
            SwitchMiniMapCommand = new DelegateCommand(() =>
            {
                DiagramViewModel.EnableMiniMap.Value = !DiagramViewModel.EnableMiniMap.Value;
                ToolBarViewModel.ToolItems2.First(x => x.Name.Value == "minimap").IsChecked = DiagramViewModel.EnableMiniMap.Value;
            });
            SwitchCombineCommand = new DelegateCommand(() =>
            {
                DiagramViewModel.EnableCombine.Value = !DiagramViewModel.EnableCombine.Value;
                ToolBarViewModel.ToolItems2.First(x => x.Name.Value == "combine").IsChecked = DiagramViewModel.EnableCombine.Value;
            });
            SwitchLayersCommand = new DelegateCommand(() =>
            {
                DiagramViewModel.EnableLayers.Value = !DiagramViewModel.EnableLayers.Value;
                ToolBarViewModel.ToolItems2.First(x => x.Name.Value == "layers").IsChecked = DiagramViewModel.EnableLayers.Value;
            });
            ShowLogCommand = new DelegateCommand(() =>
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(System.IO.Path.Combine(boilersGraphics.Helpers.Path.GetRoamingDirectory(), "dhq_boiler\\boilersGraphics\\Logs\\boilersGraphics.log"))
                {
                    UseShellExecute = true
                };
                p.Start();
                UpdateStatisticsCountOpenApplicationLog();
            });
            SetLogLevelCommand = new DelegateCommand<LogLevel>(parameter =>
            {
                LogLevel.Value = parameter;
            });
            SwitchLanguageCommand = new DelegateCommand<string>(parameter =>
            {
                ResourceService.Current.ChangeCulture(parameter);

                ToolBarViewModel.ReinitializeToolItems();
            });
            PostNewIssueCommand = new DelegateCommand(() =>
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo("https://github.com/dhq-boiler/boiler-s-Graphics/issues/new")
                {
                    UseShellExecute = true
                };
                p.Start();
            });

            SnapPower.Value = 10;

            IncrementNumberOfBoots();
            TerminalInfo.Value = CreateTerminalInfo();

            DiagramViewModel.Initialize(false);

            var updateTicks = 0L;

            var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var dao = new StatisticsDao();
            var statistics = dao.FindBy(new Dictionary<string, object>() { { "ID", id } });
            var statisticsObj = statistics.First();
            Statistics.Value = statisticsObj;
            updateTicks = statisticsObj.UptimeTicks;

            LogLevel.Subscribe(x =>
            {
                foreach (var rule in LogManager.Configuration.LoggingRules)
                {
                    rule.EnableLoggingForLevel(x);
                }
                LogManager.ReconfigExistingLoggers();

                var dao = new LogSettingDao();
                var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
                var logSettings = dao.FindBy(new Dictionary<string, object>() { { "ID", id } });
                if (logSettings.Count() == 1)
                {
                    var finished = false;
                    do
                    {
                        try
                        {
                            var logSetting = logSettings.First();
                            logSetting.LogLevel = LogLevel.Value.ToString();
                            dao.Update(logSetting);
                            finished = true;
                        }
                        catch (SQLiteException ex)
                        {
                            LogManager.GetCurrentClassLogger().Warn(ex);
                            LogManager.GetCurrentClassLogger().Warn("The app will continue to update logSettings after sleep 10 seconds of sleep.");
                            Thread.Sleep(10000);
                        }
                    }
                    while (!finished);
                }

                LogManager.GetCurrentClassLogger().Info($"ログレベルが変更されました。変更後：{x}");
                UpdateStatisticsCountLogLevelChanged();
            })
            .AddTo(_CompositeDisposable);

            Observable.Interval(TimeSpan.FromSeconds(1))
                      .Subscribe(_ =>
                      {
                          Statistics.Value.UptimeTicks = ((DateTime.Now - _StartUpTime) + TimeSpan.FromTicks(updateTicks)).Ticks;
                      })
                      .AddTo(_CompositeDisposable);

            Observable.Interval(TimeSpan.FromSeconds(10))
                      .Subscribe(_ =>
                      {
                          try
                          {
                              var dao = new StatisticsDao();
                              dao.Update(Statistics.Value);
                          }
                          catch (SQLiteException ex)
                          {
                              LogManager.GetCurrentClassLogger().Warn(ex);
                          }
                      })
                      .AddTo(_CompositeDisposable);

            ResourceService.Current.ChangeCulture(CultureInfo.CurrentCulture.Name);
        }

        private static void ConfigureNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            NLog.Targets.FileTarget fileTarget = new NLog.Targets.FileTarget("fileTarget")
            {
                FileName = $"{Helpers.Path.GetRoamingDirectory()}\\dhq_boiler\\boilersGraphics\\Logs\\boilersGraphics.log",
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                ArchiveFileName = $"{Helpers.Path.GetRoamingDirectory()}\\dhq_boiler\\boilersGraphics\\Logs\\boilersGraphics_{{#}}.log",
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Date,
                ArchiveDateFormat = "yyyy-MM-dd",
                Encoding = Encoding.UTF8
            };
            config.AddTarget(fileTarget);
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, fileTarget);
            NLog.Targets.FileTarget fileErrTarget = new NLog.Targets.FileTarget("fileErrTarget")
            {
                FileName = $"{Helpers.Path.GetRoamingDirectory()}\\dhq_boiler\\boilersGraphics\\Logs\\boilersGraphics_error.log",
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                ArchiveFileName = $"{Helpers.Path.GetRoamingDirectory()}\\dhq_boiler\\boilersGraphics\\Logs\\boilersGraphics_error_{{#}}.log",
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Date,
                ArchiveDateFormat = "yyyy-MM-dd",
                Encoding = Encoding.UTF8
            };
            config.AddTarget(fileErrTarget);
            config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, fileErrTarget);
            NLog.Targets.ConsoleTarget consoleTarget = new NLog.Targets.ConsoleTarget("consoleTarget")
            {
                Layout = "${longdate}[${level}]${message}"
            };
            config.AddTarget(consoleTarget);
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);
            NLog.Targets.DebuggerTarget debuggerTarget = new NLog.Targets.DebuggerTarget("debuggerTarget")
            {
                Layout = "${longdate}[${level}]${message}"
            };
            config.AddTarget(debuggerTarget);
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, debuggerTarget);
            NLog.LogManager.Configuration = config;
        }

        private DateTime? PickoutLatestPrivacyPolicyDateOfEnactment()
        {
            var privacyPolicyUrl = "https://raw.githubusercontent.com/dhq-boiler/boiler-s-Graphics/master/PrivacyPolicy.md";
            using (var client = new WebClient())
            {
                var markdown = client.DownloadString(privacyPolicyUrl);
                var lines = markdown.Split("\n");
                foreach (var line in lines.Reverse())
                {
                    var regex = new Regex("^改定：(?<year>\\d+?)年(?<month>\\d+?)月(?<day>\\d+?)日$");
                    if (regex.IsMatch(line))
                    {
                        var mc = regex.Match(line);
                        return DateTime.Parse($"{mc.Groups["year"].Value}/{mc.Groups["month"].Value}/{mc.Groups["day"].Value}");
                    }
                    regex = new Regex("^制定：(?<year>\\d+?)年(?<month>\\d+?)月(?<day>\\d+?)日$");
                    if (regex.IsMatch(line))
                    {
                        var mc = regex.Match(line);
                        return DateTime.Parse($"{mc.Groups["year"].Value}/{mc.Groups["month"].Value}/{mc.Groups["day"].Value}");
                    }
                }
            }
            return null;
        }

        private PrivacyPolicyAgreement PickoutPrivacyPolicyAgreementTop1()
        {
            var dao = new PrivacyPolicyAgreementDao();
            var list = dao.GetAgree();
            return list.FirstOrDefault();
        }

        private PrivacyPolicyAgreement PickoutPrivacyPolicyAgreementTop1AgreeOrDisagree()
        {
            var dao = new PrivacyPolicyAgreementDao();
            var list = dao.GetAgreeOrDisagree();
            return list.FirstOrDefault();
        }

        private void UpdateStatisticsCountLogLevelChanged()
        {
            var statistics = Statistics.Value;
            statistics.NumberOfLogLevelChanges++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private static void UpdateStatisticsCountOpenApplicationLog()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            statistics.NumberOfTimesTheApplicationLogWasDisplayed++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void ManagebGDB()
        {
            var dvManager = new DataVersionManager();
            dvManager.CurrentConnection = ConnectionManager.DefaultConnection;
            dvManager.Mode = VersioningStrategy.ByTick;
            dvManager.RegisterChangePlan(new ChangePlan_bG_VersionOrigin());
            dvManager.RegisterChangePlan(new ChangePlan_bG_Version1());
            dvManager.RegisterChangePlan(new ChangePlan_bG_Version2());
            dvManager.RegisterChangePlan(new ChangePlan_bG_Version3());
            dvManager.RegisterChangePlan(new ChangePlan_bG_Version4());
            dvManager.RegisterChangePlan(new ChangePlan_bG_Version5());
            dvManager.FinishedToUpgradeTo += DvManager_FinishedToUpgradeTo;

            dvManager.UpgradeToTargetVersion();
        }

        private void DvManager_FinishedToUpgradeTo(object sender, ModifiedEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Info($"Heavy Modifying AppDB Count : {e.ModifiedCount}");

            if (e.ModifiedCount > 0)
            {
                SQLiteBaseDao<Dummy>.Vacuum(ConnectionManager.DefaultConnection);
            }
        }

        private void IncrementNumberOfBoots()
        {
            var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var statisticsDao = new StatisticsDao();
            var statistics = statisticsDao.FindBy(new Dictionary<string, object>() { { "ID", id } });
            if (statistics.Count() == 0)
            {
                var newStatistics = new Models.Statistics();
                newStatistics.ID = id;
                newStatistics.NumberOfBoots = 1;
                statisticsDao.Insert(newStatistics);
            }
            else
            {
                var existStatistics = statistics.First();
                existStatistics.NumberOfBoots += 1;
                statisticsDao.Update(existStatistics);
            }
        }

        private TerminalInfo CreateTerminalInfo()
        {
            var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var terminalInfoDao = new TerminalInfoDao();
            var terminalInfos = terminalInfoDao.FindBy(new Dictionary<string, object>() { { "ID", id } });
            if (terminalInfos.Count() == 0)
            {
                var newTerminalInfo = new Models.TerminalInfo();
                newTerminalInfo.ID = id;
                newTerminalInfo.TerminalId = Guid.NewGuid();
                newTerminalInfo.BuildComposition = GetBuildComposition();
                terminalInfoDao.Insert(newTerminalInfo);
                return newTerminalInfo;
            }
            else
            {
                var terminalInfo = terminalInfos.First();
                return terminalInfo;
            }
        }

        private string GetBuildComposition()
        {
#if DEBUG
            return "Debug";
#else
            return "Production";
#endif
        }

        public void ClearCurrentOperationAndDetails()
        {
            CurrentOperation.Value = string.Empty;
            Details.Value = string.Empty;
        }

        public DiagramViewModel DiagramViewModel
        {
            get { return _DiagramViewModel; }
            set { SetProperty(ref _DiagramViewModel, value); }
        }

        public ToolBarViewModel ToolBarViewModel
        {
            get { return _ToolBarViewModel; }
            set { SetProperty(ref _ToolBarViewModel, value); }
        }

        public ReactivePropertySlim<string> CurrentOperation { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<string> Details { get; } = new ReactivePropertySlim<string>();
        public ReactivePropertySlim<string> Message { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<double> SnapPower { get; } = new ReactivePropertySlim<double>();

        public ReactiveCollection<double> EdgeThicknessOptions { get; } = new ReactiveCollection<double>();

        public ReactivePropertySlim<string> Title { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<Models.Statistics> Statistics { get; } = new ReactivePropertySlim<Models.Statistics>();

        public ReactivePropertySlim<TerminalInfo> TerminalInfo { get; } = new ReactivePropertySlim<TerminalInfo>();

        public IOperationController Controller { get; } = new OperationController();

        public OperationRecorder Recorder { get; }

        public ReactivePropertySlim<LogLevel> LogLevel { get; } = new ReactivePropertySlim<LogLevel>();

        public DelegateCommand<object> DeleteSelectedItemsCommand { get; private set; }

        public DelegateCommand<DiagramViewModel> SelectColorCommand { get; }

        public DelegateCommand<DiagramViewModel> SelectFillColorCommand { get; }

        public DelegateCommand ExitApplicationCommand { get; }

        public DelegateCommand SwitchMiniMapCommand { get; }

        public DelegateCommand SwitchCombineCommand { get; }

        public DelegateCommand SwitchLayersCommand { get; }

        public DelegateCommand SwitchBrushThicknessCommand { get; }

        public DelegateCommand ShowLogCommand { get; }

        public DelegateCommand ShowVersionCommand { get; }

        public DelegateCommand<LogLevel> SetLogLevelCommand { get; }

        public DelegateCommand ShowStatisticsCommand { get; }

        public DelegateCommand<string> SwitchLanguageCommand { get; }

        public DelegateCommand PostNewIssueCommand { get; }

        public DelegateCommand ShowPrivacyPolicyCommand { get; }

        private void ExecuteDeleteSelectedItemsCommand(object parameter)
        {
            var itemsToRemove = DiagramViewModel.SelectedItems.Value.ToList();
            foreach (var selectedItem in itemsToRemove)
            {
                DiagramViewModel.RemoveItemCommand.Execute(selectedItem);
            }
        }

#region IDisposable

        public void Dispose()
        {
            _CompositeDisposable.Dispose();
            Instance = null;
        }

#endregion //IDisposable
    }
}
