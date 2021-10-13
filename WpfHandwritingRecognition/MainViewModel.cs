using Microsoft.Ink;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfHandwritingRecognition
{
    public class MainViewModel : NotifyPropertyBase
    {

        public MainViewModel()
        {
            _candidates = new ObservableCollection<string>();
            _strokes = new StrokeCollection();
            _drawingAttributes = new System.Windows.Ink.DrawingAttributes
            {
                Color = Colors.DarkBlue,
                FitToCurve = true,
                IgnorePressure = false,
                IsHighlighter = false,
                StylusTip = StylusTip.Ellipse,
                Width = 18,
                Height = 18,
            };
        }

        private StrokeCollection _strokes;

        public StrokeCollection Strokes
        {
            get => _strokes;
            set => SetProperty(ref _strokes, value);
        }

        private ObservableCollection<string> _candidates;

        public ObservableCollection<string> Candidates
        {
            get => _candidates;
            set => SetProperty(ref _candidates, value);
        }

        private System.Windows.Ink.DrawingAttributes _drawingAttributes;
        public System.Windows.Ink.DrawingAttributes DrawingAttributes
        {
            get => _drawingAttributes;
            set => SetProperty(ref _drawingAttributes, value);
        }       

        public ICommand Clear
        {
            get
            {
                return new RelayCommand((x) =>
               {
                   Strokes.Clear();
                   Candidates.Clear();
               });
            }
        }
        public ICommand Recognize
        {
            get
            {
                return new RelayCommand((x) =>
                {
                    Candidates.Clear();
                    if (Strokes.Count == 0) return;
                    using (var stream = new MemoryStream())
                    {
                        Strokes.Save(stream);
                        var ink = new Ink();
                        ink.Load(stream.ToArray());
                        using (var context = new RecognizerContext())
                        {
                            context.Strokes = ink.Strokes;
                            var result = context.Recognize(out RecognitionStatus status);
                            if (status == RecognitionStatus.NoError)
                            {
                                foreach (var candidate in result.GetAlternatesFromSelection())
                                {
                                    Candidates.Add(candidate.ToString());
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
