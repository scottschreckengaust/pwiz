using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using pwiz.Common.Chemistry;
using pwiz.Common.Collections;
using pwiz.MSGraph;
using pwiz.Skyline.Alerts;
using pwiz.Skyline.Controls.SeqNode;
using pwiz.Skyline.EditUI;
using pwiz.Skyline.Model;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.Hibernate;
using pwiz.Skyline.Model.Lib;
using pwiz.Skyline.Util;
using ZedGraph;

namespace pwiz.Skyline.Controls
{
    public partial class IsotopeDistForm : DockableFormEx
    {
        private bool _updating;
        private SequenceTree _sequenceTree;
        private MessageBoxHelper _helper;
        private KeyValuePair<double?, double?> _isolationRange;
        private FragmentedMolecule _fragmentedMolecule = FragmentedMolecule.EMPTY;
        private FragmentedMolecule.Settings _settings = FragmentedMolecule.Settings.DEFAULT;
        public IsotopeDistForm(SkylineWindow skylineWindow)
        {
            InitializeComponent();
            Settings = FragmentedMolecule.Settings.FromSrmSettings(skylineWindow.Document.Settings);
            SkylineWindow = skylineWindow;
            comboFragmentIonType.Items.AddRange(new object[]
            {
                IonType.precursor,
                IonType.a,
                IonType.b,
                IonType.c,
                IonType.x,
                IonType.y,
                IonType.z,
                IonType.custom
            });
            msGraphControlPrecursor.GraphPane.Legend.IsVisible = false;
            msGraphControlFragment.GraphPane.Legend.IsVisible = false;
            _helper = new MessageBoxHelper(this, true);
            AttachTextboxCommit(tbxPrecursorCharge, () =>
            {
                int charge;
                if (_helper.ValidateNumberTextBox(tbxPrecursorCharge, null, null, out charge))
                {
                    FragmentedMolecule = FragmentedMolecule.ChangePrecursorCharge(charge);
                }
            });
            AttachTextboxCommit(tbxFragmentCharge, () =>
            {
                int charge;
                if (_helper.ValidateNumberTextBox(tbxFragmentCharge, null, null, out charge))
                {
                    FragmentedMolecule = FragmentedMolecule.ChangeFragmentCharge(charge);
                }
            });
            AttachTextboxCommit(tbxMassResolution, () =>
            {
                double massResolution;
                if (_helper.ValidateDecimalTextBox(tbxMassResolution, out massResolution))
                {
                    Settings = Settings.ChangeMassResolution(massResolution);
                }
            });
            AttachTextboxCommit(tbxChemicalFormula, () =>
            {
                if (tbxChemicalFormula.Text != FragmentedMolecule.PrecursorFormula.ToString())
                {
                    FragmentedMolecule = FragmentedMolecule.ChangePrecursorFormula(Molecule.Parse(tbxChemicalFormula.Text));
                }
            });
            AttachTextboxCommit(tbxFragmentFormula, () =>
            {
                if (tbxFragmentFormula.Text != FragmentedMolecule.FragmentFormula.ToString())
                {
                    FragmentedMolecule = FragmentedMolecule.ChangeFragmentFormula(Molecule.Parse(tbxFragmentFormula.Text));
                }
            });
            AttachTextboxCommit(tbxIsolationLower, CommitIsolation);
            AttachTextboxCommit(tbxIsolationUpper, CommitIsolation);
            AttachTextboxCommit(tbxPeptideSequence, CommitPeptideSequence);
            AttachTextboxCommit(tbxFragmentIonOrdinal, CommitFragmentIonOrdinal);
            AttachTextboxCommit(tbxPrecursorMassShift, () =>
            {
                double massShift;
                if (_helper.ValidateDecimalTextBox(tbxPrecursorCharge, out massShift))
                {
                    FragmentedMolecule = FragmentedMolecule.ChangePrecursorMassShift(
                        massShift, FragmentedMolecule.PrecursorMassType);
                }
            });
            AttachTextboxCommit(tbxFragmentMassShift, () =>
            {
                double massShift;
                if (_helper.ValidateDecimalTextBox(tbxFragmentMassShift, out massShift))
                {
                    FragmentedMolecule = FragmentedMolecule.ChangeFragmentMassShift(
                        massShift, FragmentedMolecule.FragmentMassType);
                }
            });
        }

        public SkylineWindow SkylineWindow { get; private set; }

        public SequenceTree SequenceTree
        {
            get { return _sequenceTree; }
            set
            {
                if (ReferenceEquals(SequenceTree, value))
                {
                    return;
                }
                if (SequenceTree != null)
                {
                    SequenceTree.AfterSelect -= SequenceTreeOnAfterSelect;
                }
                _sequenceTree = value;
                if (SequenceTree != null)
                {
                    SequenceTree.AfterSelect += SequenceTreeOnAfterSelect;
                }
            }
        }

        public FragmentedMolecule FragmentedMolecule
        {
            get { return _fragmentedMolecule; }
            set
            {
                if (Equals(FragmentedMolecule, value))
                {
                    return;
                }
                _fragmentedMolecule = value;
                UpdateControls();
            }
        }

        public FragmentedMolecule.Settings Settings
        {
            get { return _settings; }
            set
            {
                if (Equals(Settings, value))
                {
                    return;
                }
                _settings = value;
                UpdateControls();
            }
        }

        public KeyValuePair<double?, double?> IsolationRange
        {
            get { return _isolationRange; }
            set
            {
                if (Equals(IsolationRange, value))
                {
                    return;
                }
                _isolationRange = value;
                UpdateControls();
            }
        }

        private void UpdateControls()
        {
            _updating = true;
            try
            {
                if (FragmentedMolecule.ModifiedSequence != null)
                {
                    tbxPeptideSequence.Text = FragmentedMolecule.ModifiedSequence.FullNames;
                }
                else
                {
                    tbxPeptideSequence.Text = string.Empty;
                }

                comboFragmentIonType.Enabled = tbxFragmentIonOrdinal.Enabled =
                    checkedListBoxLosses.Enabled = FragmentedMolecule.ModifiedSequence != null;
                tbxPrecursorCharge.Text = FragmentedMolecule.PrecursorCharge.ToString();
                tbxPrecursorMassShift.Text = FragmentedMolecule.PrecursorMassShift
                    .ToString(Formats.RoundTrip, CultureInfo.CurrentCulture);
                tbxChemicalFormula.Text = FragmentedMolecule.PrecursorFormula.ToString();
                comboFragmentIonType.SelectedItem = FragmentedMolecule.FragmentIonType;
                tbxFragmentIonOrdinal.Text = 0 == FragmentedMolecule.FragmentOrdinal
                    ? String.Empty : FragmentedMolecule.FragmentOrdinal.ToString();
                tbxFragmentIonOrdinal.Enabled = FragmentedMolecule.FragmentIonType != IonType.custom &&
                                                FragmentedMolecule.FragmentIonType != IonType.precursor;
                tbxFragmentFormula.Text = FragmentedMolecule.FragmentFormula.ToString();
                tbxFragmentCharge.Text = FragmentedMolecule.FragmentCharge.ToString();
                tbxFragmentMassShift.Text = FragmentedMolecule.FragmentMassShift
                    .ToString(Formats.RoundTrip, CultureInfo.CurrentCulture);
                UpdateLosses();
                tbxMassResolution.Text = Settings.MassResolution.ToString(CultureInfo.CurrentCulture);
            }
            finally
            {
                _updating = false;
            }
            UpdateGraph();
        }

        private void UpdateLosses()
        {
            checkedListBoxLosses.Items.Clear();
            if (null == FragmentedMolecule.ModifiedSequence)
            {
                return;
            }
            var fragmentSequence = FragmentedMolecule.GetFragmentSequence(FragmentedMolecule.ModifiedSequence,
                FragmentedMolecule.FragmentIonType, FragmentedMolecule.FragmentOrdinal);
            var potentialLosses =
                fragmentSequence.GetModifications().SelectMany(modification 
                    => modification.StaticMod.Losses ?? Enumerable.Empty<FragmentLoss>())
                    .ToArray();
            var lossCounts = FragmentedMolecule.FragmentLosses.ToLookup(loss => loss)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
            foreach (var loss in potentialLosses)
            {
                checkedListBoxLosses.Items.Add(loss);
                int count;
                if (lossCounts.TryGetValue(loss, out count))
                {
                    checkedListBoxLosses.SetItemChecked(checkedListBoxLosses.Items.Count - 1, true);
                    if (count > 1)
                    {
                        lossCounts[loss] = count - 1;
                    }
                    else
                    {
                        lossCounts.Remove(loss);
                    }
                }
            }
        }

        private void UpdateGraph()
        {
            var massInfoPrecursor = new MassInfo(Settings.GetMassDistribution(FragmentedMolecule.PrecursorFormula,
                    FragmentedMolecule.PrecursorMassShift, FragmentedMolecule.PrecursorCharge),
                Settings.GetMonoMass(FragmentedMolecule.PrecursorFormula, FragmentedMolecule.PrecursorMassShift, FragmentedMolecule.PrecursorCharge),
                FragmentedMolecule.PrecursorCharge);
            var massInfoGrid = massInfoPrecursor;
            if (FragmentedMolecule.FragmentFormula.Count == 0)
            {
                splitContainerGraph.Panel2Collapsed = true;
            }
            else
            {
                splitContainerGraph.Panel2Collapsed = false;
                try
                {
                    var massDistribution = FragmentedMolecule.GetFragmentDistribution(Settings, IsolationRange.Key, IsolationRange.Value);
                    massInfoGrid = new MassInfo(massDistribution,
                        Settings.GetMonoMass(FragmentedMolecule.FragmentFormula, FragmentedMolecule.FragmentMassShift, FragmentedMolecule.FragmentCharge),
                        FragmentedMolecule.FragmentCharge);
                    var fragmentPoints = ToPointPairList(massDistribution);
                    msGraphControlFragment.GraphPane.CurveList.Clear();
                    msGraphControlFragment.AddGraphItem(msGraphControlFragment.GraphPane, new GraphItem
                    {
                        Color = Color.Black,
                        Points = fragmentPoints,
                        Title = "Fragment"
                    });
                    msGraphControlFragment.GraphPane.Title.Text = null;
                }
                catch (Exception e)
                {
                    msGraphControlFragment.GraphPane.Title.Text = e.Message;
                }
            }
            UpdateDataGrid(ReferenceEquals(massInfoGrid, massInfoPrecursor), massInfoGrid);
            msGraphControlPrecursor.GraphPane.CurveList.Clear();
            msGraphControlPrecursor.GraphPane.GraphObjList.Clear();
            msGraphControlPrecursor.AddGraphItem(msGraphControlPrecursor.GraphPane, new GraphItem
            {
                Color = Color.Black,
                Points = ToPointPairList(massInfoPrecursor.MassDistribution),
                Title = "Precursor",
            });
            if (IsolationRange.Key.HasValue || IsolationRange.Value.HasValue)
            {
                var isolationMin = IsolationRange.Key.GetValueOrDefault(-1e6);
                var isolationMax = IsolationRange.Value.GetValueOrDefault(1e6);
                var isolationRect = new BoxObj(isolationMin, 0, isolationMax - isolationMin, 1, Color.MediumPurple,
                    Color.MediumPurple)
                {
                    IsClippedToChartRect = true,
                    ZOrder = ZOrder.E_BehindCurves,
                    Location = {CoordinateFrame = CoordType.XScaleYChartFraction}
                };
                msGraphControlPrecursor.GraphPane.GraphObjList.Add(isolationRect);
            }
            msGraphControlFragment.Invalidate();
            msGraphControlPrecursor.Invalidate();
            tbxMassResolution.Text = Settings.MassResolution.ToString(CultureInfo.CurrentCulture);
            msGraphControlPrecursor.GraphPane.XAxis.Title.Text = "Precursor M/Z";
            msGraphControlFragment.GraphPane.XAxis.Title.Text = "Fragment M/Z";
        }

        private void UpdateDataGrid(bool precursor, MassInfo massInfo)
        {
            if (massInfo.Charge == 0)
            {
                lblMonoisotopicMass.Text = "Monoisotopic Mass";
                lblAverageMass.Text = "Average Mass";
                colMass.HeaderText = precursor ? "Precursor Mass" : "Fragment Mass";
            }
            else
            {
                lblMonoisotopicMass.Text = "Monoisotopic M/Z";
                lblAverageMass.Text = "Average M/Z";
                colMass.HeaderText = precursor ? "Precursor M/Z" : "Fragment M/Z";
            }
            dataGridViewMassIntensity.Rows.Clear();
            var totalAbundance = 0.0;
            var totalMassAbundance = 0.0;
            foreach (var entry in massInfo.MassDistribution.OrderBy(kvp => kvp.Key))
            {
                var row = dataGridViewMassIntensity.Rows[dataGridViewMassIntensity.Rows.Add()];
                row.Cells[colMass.Index].Value = entry.Key;
                row.Cells[colIntensity.Index].Value = entry.Value;
                totalAbundance += entry.Value;
                totalMassAbundance += entry.Key * entry.Value;
            }
            double averageMass;
            if (totalAbundance == 0)
            {
                averageMass = 0;
            }
            else
            {
                averageMass = totalMassAbundance / totalAbundance;
                if (massInfo.Charge != 0)
                {
                    averageMass *= Math.Sign(massInfo.Charge);
                }
            }
            tbxAverageMass.Text = averageMass.ToString(CultureInfo.CurrentCulture);
            tbxMonoMass.Text = massInfo.MonoMass.ToString(CultureInfo.CurrentCulture);
        }

        private PointPairList ToPointPairList(IEnumerable<KeyValuePair<double, double>> massDistribution)
        {
            var entries = massDistribution.OrderBy(entry => entry.Key).ToArray();
            return new PointPairList(entries.Select(entry => entry.Key).ToArray(),
                entries.Select(entry => entry.Value).ToArray());
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (SkylineWindow != null)
            {
                SkylineWindow.DocumentUIChangedEvent += SkylineWindowOnDocumentUIChangedEvent;
                SequenceTree = SkylineWindow.SequenceTree;
                UpdateSelection();
            }
        }

        private void SkylineWindowOnDocumentUIChangedEvent(object sender, DocumentChangedEventArgs documentChangedEventArgs)
        {
            SequenceTree = SkylineWindow.SequenceTree;
            Settings = Settings.ChangeIsotopeAbundances(SkylineWindow.Document.Settings.TransitionSettings.FullScan.IsotopeAbundances);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (SkylineWindow != null)
            {
                SkylineWindow.DocumentUIChangedEvent -= SkylineWindowOnDocumentUIChangedEvent;
            }
            SequenceTree = null;
            base.OnHandleDestroyed(e);
        }
        private void SequenceTreeOnAfterSelect(object sender, TreeViewEventArgs treeViewEventArgs)
        {
            if (!cbxTrackSelection.Checked || _updating)
            {
                return;
            }
            UpdateSelection();
        }

        public void UpdateSelection()
        {
            var selectedNode = SkylineWindow.SequenceTree.SelectedNode as SrmTreeNode;
            if (selectedNode == null)
            {
                return;
            }
            var peptideTreeNode = selectedNode.GetNodeOfType<PeptideTreeNode>();
            if (peptideTreeNode == null)
            {
                return;
            }
            var peptideDocNode = peptideTreeNode.DocNode;
            var document = SkylineWindow.DocumentUI;
            var precursorTreeNode = selectedNode.GetNodeOfType<TransitionGroupTreeNode>();
            var transitionTreeNode = selectedNode.GetNodeOfType<TransitionTreeNode>();
            FragmentedMolecule = FragmentedMolecule.GetFragmentedMolecule(document.Settings, peptideDocNode,
                precursorTreeNode == null ? null : precursorTreeNode.DocNode,
                transitionTreeNode == null ? null : transitionTreeNode.DocNode);
        }

        public void CommitIsolation()
        {
            double isolationLower = double.MinValue;
            double isolationUpper = double.MaxValue;
            if (!string.IsNullOrEmpty(tbxIsolationLower.Text))
            {
                if (!_helper.ValidateDecimalTextBox(tbxIsolationLower, out isolationLower))
                {
                    return;
                }
            }
            if (!string.IsNullOrEmpty(tbxIsolationUpper.Text))
            {
                if (!_helper.ValidateDecimalTextBox(tbxIsolationUpper, out isolationUpper))
                {
                    return;
                }
            }
            IsolationRange = new KeyValuePair<double?, double?>(
                isolationLower <= double.MinValue ? (double?) null : isolationLower,
                isolationUpper >= Double.MaxValue ? (double?) null : isolationUpper);
        }

        private void CommitFragmentIonOrdinal()
        {
            if (string.IsNullOrEmpty(tbxFragmentFormula.Text))
            {
                return;
            }
            int ordinal;
            if (!_helper.ValidateNumberTextBox(tbxFragmentIonOrdinal, 1,
                FragmentedMolecule.ModifiedSequence.GetUnmodifiedSequence().Length, out ordinal))
            {
                return;
            }
            FragmentedMolecule = FragmentedMolecule.ChangeFragmentIon(FragmentedMolecule.FragmentIonType, ordinal);
        }

        private class GraphItem : IMSGraphItemInfo
        {
            public string Title { get; set; }
            public Color Color { get; set; }
            public float LineWidth { get { return LineBase.Default.Width; } }

            void CustomizeAxis(Axis axis)
            {
                axis.Title.FontSpec.Family = "Arial";
                axis.Title.FontSpec.Size = 14;
                axis.Color = axis.Title.FontSpec.FontColor = Color.Black;
                axis.Title.FontSpec.Border.IsVisible = false;
            }

            public PointAnnotation AnnotatePoint(PointPair point)
            {
                return null;
            }

            public void AddAnnotations(MSGraphPane graphPane, Graphics g, MSPointList pointList, GraphObjList annotations)
            {
            }

            public MSGraphItemType GraphItemType
            {
                get { return MSGraphItemType.chromatogram; }
            }

            public MSGraphItemDrawMethod GraphItemDrawMethod { get { return MSGraphItemDrawMethod.stick; } }

            public void CustomizeXAxis(Axis axis)
            {
                axis.Title.Text = "M/Z";
                CustomizeAxis(axis);
            }

            public void CustomizeYAxis(Axis axis)
            {
                axis.Title.Text = "Intensity";
                CustomizeAxis(axis);
            }

            public IPointList Points
            {
                get;
                set;
            }
        }

        private void checkedListBoxLosses_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_updating)
            {
                return;
            }
            BeginInvoke(new Action(() =>
            {
                FragmentedMolecule = FragmentedMolecule.ChangeFragmentLosses(checkedListBoxLosses.CheckedItems.OfType<FragmentLoss>());
            }));
        }

        private void CommitPeptideSequence()
        {
            var sequence = tbxPeptideSequence.Text.Trim();
            if (string.IsNullOrEmpty(sequence))
            {
                if (FragmentedMolecule.ModifiedSequence != null)
                {
                    FragmentedMolecule = FragmentedMolecule.ChangeModifiedSequence(null);
                }
                return;
            }
            var peptideLibraryKey = new PeptideLibraryKey(sequence, 0);
            var modifications = new List<ModifiedSequence.Modification>();
            var peptideModifications = SkylineWindow.Document.Settings.PeptideSettings.Modifications;
            foreach (var modificationEntry in peptideLibraryKey.GetModifications())
            {
                var modificationName = modificationEntry.Value;
                var staticMod = FindModificationByName(peptideModifications, modificationName);
                if (staticMod == null)
                {
                    string message = string.Format("Unable to find a modification named '{0}'.", modificationName);
                    MessageDlg.Show(this, message);
                    tbxPeptideSequence.Focus();
                    return;
                }
                modifications.Add(new ModifiedSequence.Modification(new ExplicitMod(modificationEntry.Key, staticMod), 0, 0));
            }
            var modifiedSequence = new ModifiedSequence(peptideLibraryKey.UnmodifiedSequence, modifications, MassType.Monoisotopic);
            FragmentedMolecule = FragmentedMolecule.ChangeModifiedSequence(modifiedSequence);
        }

        private StaticMod FindModificationByName(PeptideModifications peptideModifications, string name)
        {
            foreach (var mod in peptideModifications.GetModificationTypes()
                .SelectMany(peptideModifications.GetModifications))
            {
                if (mod.Name == name)
                {
                    return mod;
                }
            }
            bool structural;
            return UniMod.GetModification(name, out structural);
        }

        private void AttachTextboxCommit(TextBox textBox, Action validator)
        {
            textBox.Leave += (sender, args) => validator();
            textBox.KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.Return)
                {
                    validator();
                }
                else if (args.KeyCode == Keys.Escape)
                {
                    UpdateControls();
                }
            };
        }

        private class MassInfo
        {
            public MassInfo(IEnumerable<KeyValuePair<double, double>> massDistribution, double monoMass, int charge)
            {
                MassDistribution = ImmutableList.ValueOf(massDistribution.OrderBy(kvp=>kvp.Key));
                MonoMass = monoMass;
                Charge = charge;
            }
            
            public ImmutableList<KeyValuePair<double, double>> MassDistribution { get; private set; }
            public double MonoMass { get; set; }
            public int Charge { get; set; }
        }

        private void comboFragmentIonType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }
            if (null == FragmentedMolecule.ModifiedSequence)
            {
                return;
            }
            var ionType = (IonType) comboFragmentIonType.SelectedItem;
            int ordinal;
            if (ionType == IonType.precursor || ionType == IonType.custom)
            {
                ordinal = 0;
            }
            else
            {
                ordinal = FragmentedMolecule.FragmentOrdinal;
                string unmodifiedSequence = FragmentedMolecule.ModifiedSequence.GetUnmodifiedSequence();
                if (ordinal <= 0)
                {
                    ordinal = 1;
                }
                else if (ordinal > unmodifiedSequence.Length)
                {
                    ordinal = unmodifiedSequence.Length;
                }
            }
            FragmentedMolecule = FragmentedMolecule.ChangeFragmentIon(ionType, ordinal);
        }

        private void GraphControlContextMenuBuilder(ZedGraphControl sender, ContextMenuStrip menuStrip, Point mousePt, ZedGraphControl.ContextMenuObjectState objState)
        {
            CopyEmfToolStripMenuItem.AddToContextMenu(sender, menuStrip);
        }
    }
}
