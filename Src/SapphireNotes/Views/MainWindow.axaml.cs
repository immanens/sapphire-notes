﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using SapphireNotes.Services;
using SapphireNotes.ViewModels;
using SapphireNotes.ViewModels.UserControls;
using Splat;

namespace SapphireNotes.Views
{
    public class MainWindow : Window
    {
        private readonly INotesService _notesService;
        private readonly List<Window> _windows = new List<Window>();
        private readonly TabControl _notesTabControl;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _notesService = Locator.Current.GetService<INotesService>();
            _notesService.Created += NoteCreated;
            _notesService.Restored += NoteRestored;

            var escapeButton = this.FindControl<Button>("escapeButton");
            escapeButton.Command = ReactiveCommand.Create(ExcapeButtonClicked);

            var quickNoteButton = this.FindControl<Button>("quickNoteButton");
            quickNoteButton.Command = ReactiveCommand.Create(QuickNoteButtonClicked);

            _notesTabControl = this.FindControl<TabControl>("noteTabs");
            _notesTabControl.SelectionChanged += NoteSelectionChanged;

            var newNoteButton = this.FindControl<Button>("newNoteButton");
            newNoteButton.Command = ReactiveCommand.Create(NewNoteButtonClicked);

            var archivedButton = this.FindControl<Button>("archivedButton");
            archivedButton.Command = ReactiveCommand.Create(ArchivedButtonClicked);

            var preferencesButton = this.FindControl<Button>("preferencesButton");
            preferencesButton.Command = ReactiveCommand.Create(PreferencesButtonClicked);

            var tipsButton = this.FindControl<Button>("tipsButton");
            tipsButton.Command = ReactiveCommand.Create(TipsButtonClicked);

            DataContextChanged += MainWindow_DataContextChanged;
        }

        private void NoteSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedVm = e.AddedItems[0] as NoteViewModel;
            selectedVm.Select();
        }

        private void Note_Edit(object sender, EventArgs e)
        {
            var window = new EditNoteWindow
            {
                DataContext = new EditNoteViewModel(_notesService, (sender as NoteViewModel).ToNote()),
                Width = 300,
                Height = 98,
                Topmost = true,
                CanResize = false
            };
            window.Show();
            window.Activate();

            _windows.Add(window);
        }

        private void Note_Delete(object sender, EventArgs e)
        {
            var window = new DeleteNoteWindow
            {
                DataContext = new DeleteNoteViewModel(_notesService, (sender as NoteViewModel).ToNote()),
                Topmost = true,
                CanResize = false
            };
            window.Show();
            window.Activate();

            _windows.Add(window);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (var window in _windows)
            {
                window.Close();
            }

            var vm = DataContext as MainWindowViewModel;
            vm.OnClosing((int)Width, (int)Height, Position.X, Position.Y);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ExcapeButtonClicked()
        {
            Close();
        }

        private void QuickNoteButtonClicked()
        {
            var window = new QuickNoteWindow
            {
                DataContext = new QuickNoteViewModel(_notesService),
                Owner = this,
                Topmost = true,
                CanResize = false
            };
            window.Show();
            window.Activate();

            _windows.Add(window);
        }

        private void NewNoteButtonClicked()
        {
            var window = new EditNoteWindow
            {
                DataContext = new EditNoteViewModel(_notesService),
                Owner = this,
                Topmost = true,
                CanResize = false
            };
            window.Show();
            window.Activate();

            _windows.Add(window);
        }

        private void NoteCreated(object sender, CreatedNoteEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            NoteViewModel noteVm = vm.AddNote(e.CreatedNote);

            _notesTabControl.SelectedItem = noteVm;
        }

        private void ArchivedButtonClicked()
        {
            var window = new ArchivedNotesWindow
            {
                DataContext = new ArchivedNotesViewModel(_notesService),
                Owner = this,
                Topmost = true
            };
            window.Show();
            window.Activate();

            _windows.Add(window);
        }

        private void NoteRestored(object sender, RestoredNoteEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            NoteViewModel noteVm = vm.AddNote(e.RestoredNote);

            _notesTabControl.SelectedItem = noteVm;
        }

        private void PreferencesButtonClicked()
        {
            var window = new PreferencesWindow
            {
                DataContext = new PreferencesViewModel(Locator.Current.GetService<IPreferencesService>(), _notesService),
                Owner = this,
                Topmost = true,
                CanResize = false
            };
            window.Saved += PreferencesSaved;
            window.Show();
            window.Activate();

            _windows.Add(window);
        }

        private void TipsButtonClicked()
        {
            var window = new TipsWindow
            {
                Owner = this,
                Topmost = true,
                CanResize = false
            };
            window.Show();
            window.Activate();

            _windows.Add(window);
        }

        private void PreferencesSaved(object sender, PreferencesSavedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            vm.PreferencesSaved(e.NotesAreDirty);
        }

        private void MainWindow_DataContextChanged(object sender, EventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            vm.NoteEditClicked += Note_Edit;
            vm.NoteDeleteClicked += Note_Delete;
        }
    }
}
