namespace pwiz.Skyline.Controls
{
    partial class IsotopeDistForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBoxFragment = new System.Windows.Forms.GroupBox();
            this.lblLosses = new System.Windows.Forms.Label();
            this.lblFragmentFormula = new System.Windows.Forms.Label();
            this.tbxFragmentFormula = new System.Windows.Forms.TextBox();
            this.lblFragmentIonType = new System.Windows.Forms.Label();
            this.comboFragmentIonType = new System.Windows.Forms.ComboBox();
            this.tbxFragmentIonOrdinal = new System.Windows.Forms.TextBox();
            this.lblFragmentIonIndex = new System.Windows.Forms.Label();
            this.tbxFragmentCharge = new System.Windows.Forms.TextBox();
            this.checkedListBoxLosses = new System.Windows.Forms.CheckedListBox();
            this.lblCharge = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbxIsolationLower = new System.Windows.Forms.TextBox();
            this.lblHyphen = new System.Windows.Forms.Label();
            this.tbxIsolationUpper = new System.Windows.Forms.TextBox();
            this.groupBoxPrecursor = new System.Windows.Forms.GroupBox();
            this.lblPeptideSequence = new System.Windows.Forms.Label();
            this.tbxPeptideSequence = new System.Windows.Forms.TextBox();
            this.lblPrecursorFormula = new System.Windows.Forms.Label();
            this.tbxChemicalFormula = new System.Windows.Forms.TextBox();
            this.tbxPrecursorCharge = new System.Windows.Forms.TextBox();
            this.lblPrecursorCharge = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dataGridViewMassIntensity = new System.Windows.Forms.DataGridView();
            this.colMass = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntensity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblAverageMass = new System.Windows.Forms.Label();
            this.tbxAverageMass = new System.Windows.Forms.TextBox();
            this.lblMonoisotopicMass = new System.Windows.Forms.Label();
            this.tbxMonoMass = new System.Windows.Forms.TextBox();
            this.splitContainerGraph = new System.Windows.Forms.SplitContainer();
            this.msGraphControlPrecursor = new pwiz.MSGraph.MSGraphControl();
            this.msGraphControlFragment = new pwiz.MSGraph.MSGraphControl();
            this.flowLayoutPanel1 = new System.Windows.Forms.Panel();
            this.lblMassResolution = new System.Windows.Forms.Label();
            this.tbxMassResolution = new System.Windows.Forms.TextBox();
            this.cbxTrackSelection = new System.Windows.Forms.CheckBox();
            this.lblPrecursorMassShift = new System.Windows.Forms.Label();
            this.tbxPrecursorMassShift = new System.Windows.Forms.TextBox();
            this.lblFragmentMassShift = new System.Windows.Forms.Label();
            this.tbxFragmentMassShift = new System.Windows.Forms.TextBox();
            this.groupBoxFragment.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxPrecursor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMassIntensity)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGraph)).BeginInit();
            this.splitContainerGraph.Panel1.SuspendLayout();
            this.splitContainerGraph.Panel2.SuspendLayout();
            this.splitContainerGraph.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxFragment
            // 
            this.groupBoxFragment.Controls.Add(this.tbxFragmentMassShift);
            this.groupBoxFragment.Controls.Add(this.lblFragmentMassShift);
            this.groupBoxFragment.Controls.Add(this.lblLosses);
            this.groupBoxFragment.Controls.Add(this.lblFragmentFormula);
            this.groupBoxFragment.Controls.Add(this.tbxFragmentFormula);
            this.groupBoxFragment.Controls.Add(this.lblFragmentIonType);
            this.groupBoxFragment.Controls.Add(this.comboFragmentIonType);
            this.groupBoxFragment.Controls.Add(this.tbxFragmentIonOrdinal);
            this.groupBoxFragment.Controls.Add(this.lblFragmentIonIndex);
            this.groupBoxFragment.Controls.Add(this.tbxFragmentCharge);
            this.groupBoxFragment.Controls.Add(this.checkedListBoxLosses);
            this.groupBoxFragment.Controls.Add(this.lblCharge);
            this.groupBoxFragment.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxFragment.Location = new System.Drawing.Point(0, 102);
            this.groupBoxFragment.Name = "groupBoxFragment";
            this.groupBoxFragment.Size = new System.Drawing.Size(784, 114);
            this.groupBoxFragment.TabIndex = 1;
            this.groupBoxFragment.TabStop = false;
            this.groupBoxFragment.Text = "Fragment";
            // 
            // lblLosses
            // 
            this.lblLosses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLosses.AutoSize = true;
            this.lblLosses.Location = new System.Drawing.Point(530, 17);
            this.lblLosses.Name = "lblLosses";
            this.lblLosses.Size = new System.Drawing.Size(43, 13);
            this.lblLosses.TabIndex = 6;
            this.lblLosses.Text = "Losses:";
            // 
            // lblFragmentFormula
            // 
            this.lblFragmentFormula.AutoSize = true;
            this.lblFragmentFormula.Location = new System.Drawing.Point(12, 56);
            this.lblFragmentFormula.Name = "lblFragmentFormula";
            this.lblFragmentFormula.Size = new System.Drawing.Size(90, 13);
            this.lblFragmentFormula.TabIndex = 8;
            this.lblFragmentFormula.Text = "Chemical formula:";
            // 
            // tbxFragmentFormula
            // 
            this.tbxFragmentFormula.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxFragmentFormula.Location = new System.Drawing.Point(11, 77);
            this.tbxFragmentFormula.Name = "tbxFragmentFormula";
            this.tbxFragmentFormula.Size = new System.Drawing.Size(506, 20);
            this.tbxFragmentFormula.TabIndex = 9;
            // 
            // lblFragmentIonType
            // 
            this.lblFragmentIonType.AutoSize = true;
            this.lblFragmentIonType.Location = new System.Drawing.Point(13, 16);
            this.lblFragmentIonType.Name = "lblFragmentIonType";
            this.lblFragmentIonType.Size = new System.Drawing.Size(94, 13);
            this.lblFragmentIonType.TabIndex = 0;
            this.lblFragmentIonType.Text = "Fragment ion type:";
            // 
            // comboFragmentIonType
            // 
            this.comboFragmentIonType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFragmentIonType.FormattingEnabled = true;
            this.comboFragmentIonType.Location = new System.Drawing.Point(16, 32);
            this.comboFragmentIonType.Name = "comboFragmentIonType";
            this.comboFragmentIonType.Size = new System.Drawing.Size(121, 21);
            this.comboFragmentIonType.TabIndex = 1;
            this.comboFragmentIonType.SelectedIndexChanged += new System.EventHandler(this.comboFragmentIonType_SelectedIndexChanged);
            // 
            // tbxFragmentIonOrdinal
            // 
            this.tbxFragmentIonOrdinal.Location = new System.Drawing.Point(143, 33);
            this.tbxFragmentIonOrdinal.Name = "tbxFragmentIonOrdinal";
            this.tbxFragmentIonOrdinal.Size = new System.Drawing.Size(121, 20);
            this.tbxFragmentIonOrdinal.TabIndex = 3;
            // 
            // lblFragmentIonIndex
            // 
            this.lblFragmentIonIndex.AutoSize = true;
            this.lblFragmentIonIndex.Location = new System.Drawing.Point(140, 17);
            this.lblFragmentIonIndex.Name = "lblFragmentIonIndex";
            this.lblFragmentIonIndex.Size = new System.Drawing.Size(105, 13);
            this.lblFragmentIonIndex.TabIndex = 2;
            this.lblFragmentIonIndex.Text = "Fragment ion ordinal:";
            // 
            // tbxFragmentCharge
            // 
            this.tbxFragmentCharge.Location = new System.Drawing.Point(270, 32);
            this.tbxFragmentCharge.Name = "tbxFragmentCharge";
            this.tbxFragmentCharge.Size = new System.Drawing.Size(121, 20);
            this.tbxFragmentCharge.TabIndex = 5;
            // 
            // checkedListBoxLosses
            // 
            this.checkedListBoxLosses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxLosses.FormattingEnabled = true;
            this.checkedListBoxLosses.Location = new System.Drawing.Point(534, 34);
            this.checkedListBoxLosses.Name = "checkedListBoxLosses";
            this.checkedListBoxLosses.Size = new System.Drawing.Size(237, 64);
            this.checkedListBoxLosses.TabIndex = 7;
            this.checkedListBoxLosses.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxLosses_ItemCheck);
            // 
            // lblCharge
            // 
            this.lblCharge.AutoSize = true;
            this.lblCharge.Location = new System.Drawing.Point(267, 16);
            this.lblCharge.Name = "lblCharge";
            this.lblCharge.Size = new System.Drawing.Size(44, 13);
            this.lblCharge.TabIndex = 4;
            this.lblCharge.Text = "Charge:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbxIsolationLower);
            this.groupBox1.Controls.Add(this.lblHyphen);
            this.groupBox1.Controls.Add(this.tbxIsolationUpper);
            this.groupBox1.Location = new System.Drawing.Point(127, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(251, 47);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Precursor Isolation";
            // 
            // tbxIsolationLower
            // 
            this.tbxIsolationLower.Location = new System.Drawing.Point(6, 21);
            this.tbxIsolationLower.Name = "tbxIsolationLower";
            this.tbxIsolationLower.Size = new System.Drawing.Size(100, 20);
            this.tbxIsolationLower.TabIndex = 0;
            // 
            // lblHyphen
            // 
            this.lblHyphen.AutoSize = true;
            this.lblHyphen.Location = new System.Drawing.Point(112, 24);
            this.lblHyphen.Name = "lblHyphen";
            this.lblHyphen.Size = new System.Drawing.Size(10, 13);
            this.lblHyphen.TabIndex = 1;
            this.lblHyphen.Text = "-";
            // 
            // tbxIsolationUpper
            // 
            this.tbxIsolationUpper.Location = new System.Drawing.Point(128, 21);
            this.tbxIsolationUpper.Name = "tbxIsolationUpper";
            this.tbxIsolationUpper.Size = new System.Drawing.Size(100, 20);
            this.tbxIsolationUpper.TabIndex = 2;
            // 
            // groupBoxPrecursor
            // 
            this.groupBoxPrecursor.Controls.Add(this.tbxPrecursorMassShift);
            this.groupBoxPrecursor.Controls.Add(this.lblPrecursorMassShift);
            this.groupBoxPrecursor.Controls.Add(this.lblPeptideSequence);
            this.groupBoxPrecursor.Controls.Add(this.tbxPeptideSequence);
            this.groupBoxPrecursor.Controls.Add(this.lblPrecursorFormula);
            this.groupBoxPrecursor.Controls.Add(this.tbxChemicalFormula);
            this.groupBoxPrecursor.Controls.Add(this.tbxPrecursorCharge);
            this.groupBoxPrecursor.Controls.Add(this.lblPrecursorCharge);
            this.groupBoxPrecursor.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxPrecursor.Location = new System.Drawing.Point(0, 0);
            this.groupBoxPrecursor.Name = "groupBoxPrecursor";
            this.groupBoxPrecursor.Size = new System.Drawing.Size(784, 102);
            this.groupBoxPrecursor.TabIndex = 0;
            this.groupBoxPrecursor.TabStop = false;
            this.groupBoxPrecursor.Text = "Precursor";
            // 
            // lblPeptideSequence
            // 
            this.lblPeptideSequence.AutoSize = true;
            this.lblPeptideSequence.Location = new System.Drawing.Point(6, 16);
            this.lblPeptideSequence.Name = "lblPeptideSequence";
            this.lblPeptideSequence.Size = new System.Drawing.Size(96, 13);
            this.lblPeptideSequence.TabIndex = 0;
            this.lblPeptideSequence.Text = "Peptide sequence:";
            // 
            // tbxPeptideSequence
            // 
            this.tbxPeptideSequence.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxPeptideSequence.Location = new System.Drawing.Point(9, 32);
            this.tbxPeptideSequence.Name = "tbxPeptideSequence";
            this.tbxPeptideSequence.Size = new System.Drawing.Size(763, 20);
            this.tbxPeptideSequence.TabIndex = 1;
            // 
            // lblPrecursorFormula
            // 
            this.lblPrecursorFormula.AutoSize = true;
            this.lblPrecursorFormula.Location = new System.Drawing.Point(7, 55);
            this.lblPrecursorFormula.Name = "lblPrecursorFormula";
            this.lblPrecursorFormula.Size = new System.Drawing.Size(90, 13);
            this.lblPrecursorFormula.TabIndex = 2;
            this.lblPrecursorFormula.Text = "Chemical formula:";
            // 
            // tbxChemicalFormula
            // 
            this.tbxChemicalFormula.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxChemicalFormula.Location = new System.Drawing.Point(9, 71);
            this.tbxChemicalFormula.Name = "tbxChemicalFormula";
            this.tbxChemicalFormula.Size = new System.Drawing.Size(539, 20);
            this.tbxChemicalFormula.TabIndex = 3;
            // 
            // tbxPrecursorCharge
            // 
            this.tbxPrecursorCharge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxPrecursorCharge.Location = new System.Drawing.Point(554, 71);
            this.tbxPrecursorCharge.Name = "tbxPrecursorCharge";
            this.tbxPrecursorCharge.Size = new System.Drawing.Size(111, 20);
            this.tbxPrecursorCharge.TabIndex = 5;
            // 
            // lblPrecursorCharge
            // 
            this.lblPrecursorCharge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPrecursorCharge.AutoSize = true;
            this.lblPrecursorCharge.Location = new System.Drawing.Point(551, 55);
            this.lblPrecursorCharge.Name = "lblPrecursorCharge";
            this.lblPrecursorCharge.Size = new System.Drawing.Size(44, 13);
            this.lblPrecursorCharge.TabIndex = 4;
            this.lblPrecursorCharge.Text = "Charge:";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 267);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dataGridViewMassIntensity);
            this.splitContainer2.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainerGraph);
            this.splitContainer2.Size = new System.Drawing.Size(784, 294);
            this.splitContainer2.SplitterDistance = 304;
            this.splitContainer2.TabIndex = 0;
            // 
            // dataGridViewMassIntensity
            // 
            this.dataGridViewMassIntensity.AllowUserToAddRows = false;
            this.dataGridViewMassIntensity.AllowUserToDeleteRows = false;
            this.dataGridViewMassIntensity.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewMassIntensity.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewMassIntensity.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMassIntensity.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMass,
            this.colIntensity});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewMassIntensity.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewMassIntensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewMassIntensity.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewMassIntensity.Name = "dataGridViewMassIntensity";
            this.dataGridViewMassIntensity.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewMassIntensity.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewMassIntensity.Size = new System.Drawing.Size(304, 240);
            this.dataGridViewMassIntensity.TabIndex = 0;
            // 
            // colMass
            // 
            this.colMass.HeaderText = "Mass";
            this.colMass.Name = "colMass";
            this.colMass.ReadOnly = true;
            // 
            // colIntensity
            // 
            this.colIntensity.HeaderText = "Intensity";
            this.colIntensity.Name = "colIntensity";
            this.colIntensity.ReadOnly = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lblAverageMass, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbxAverageMass, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblMonoisotopicMass, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbxMonoMass, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 240);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(304, 54);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // lblAverageMass
            // 
            this.lblAverageMass.AutoSize = true;
            this.lblAverageMass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAverageMass.Location = new System.Drawing.Point(3, 27);
            this.lblAverageMass.Name = "lblAverageMass";
            this.lblAverageMass.Size = new System.Drawing.Size(146, 27);
            this.lblAverageMass.TabIndex = 0;
            this.lblAverageMass.Text = "Average Mass";
            this.lblAverageMass.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbxAverageMass
            // 
            this.tbxAverageMass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxAverageMass.Location = new System.Drawing.Point(155, 30);
            this.tbxAverageMass.Name = "tbxAverageMass";
            this.tbxAverageMass.ReadOnly = true;
            this.tbxAverageMass.Size = new System.Drawing.Size(146, 20);
            this.tbxAverageMass.TabIndex = 1;
            // 
            // lblMonoisotopicMass
            // 
            this.lblMonoisotopicMass.AutoSize = true;
            this.lblMonoisotopicMass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMonoisotopicMass.Location = new System.Drawing.Point(3, 0);
            this.lblMonoisotopicMass.Name = "lblMonoisotopicMass";
            this.lblMonoisotopicMass.Size = new System.Drawing.Size(146, 27);
            this.lblMonoisotopicMass.TabIndex = 2;
            this.lblMonoisotopicMass.Text = "Monoisotopic Mass";
            this.lblMonoisotopicMass.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbxMonoMass
            // 
            this.tbxMonoMass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxMonoMass.Location = new System.Drawing.Point(155, 3);
            this.tbxMonoMass.Name = "tbxMonoMass";
            this.tbxMonoMass.ReadOnly = true;
            this.tbxMonoMass.Size = new System.Drawing.Size(146, 20);
            this.tbxMonoMass.TabIndex = 3;
            // 
            // splitContainerGraph
            // 
            this.splitContainerGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerGraph.Location = new System.Drawing.Point(0, 0);
            this.splitContainerGraph.Name = "splitContainerGraph";
            this.splitContainerGraph.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerGraph.Panel1
            // 
            this.splitContainerGraph.Panel1.Controls.Add(this.msGraphControlPrecursor);
            // 
            // splitContainerGraph.Panel2
            // 
            this.splitContainerGraph.Panel2.Controls.Add(this.msGraphControlFragment);
            this.splitContainerGraph.Size = new System.Drawing.Size(476, 294);
            this.splitContainerGraph.SplitterDistance = 139;
            this.splitContainerGraph.TabIndex = 0;
            // 
            // msGraphControlPrecursor
            // 
            this.msGraphControlPrecursor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.msGraphControlPrecursor.EditButtons = System.Windows.Forms.MouseButtons.Left;
            this.msGraphControlPrecursor.EditModifierKeys = System.Windows.Forms.Keys.None;
            this.msGraphControlPrecursor.IsEnableVPan = false;
            this.msGraphControlPrecursor.IsEnableVZoom = false;
            this.msGraphControlPrecursor.IsShowCopyMessage = false;
            this.msGraphControlPrecursor.IsZoomOnMouseCenter = true;
            this.msGraphControlPrecursor.Location = new System.Drawing.Point(0, 0);
            this.msGraphControlPrecursor.Name = "msGraphControlPrecursor";
            this.msGraphControlPrecursor.ScrollGrace = 0D;
            this.msGraphControlPrecursor.ScrollMaxX = 0D;
            this.msGraphControlPrecursor.ScrollMaxY = 0D;
            this.msGraphControlPrecursor.ScrollMaxY2 = 0D;
            this.msGraphControlPrecursor.ScrollMinX = 0D;
            this.msGraphControlPrecursor.ScrollMinY = 0D;
            this.msGraphControlPrecursor.ScrollMinY2 = 0D;
            this.msGraphControlPrecursor.Size = new System.Drawing.Size(476, 139);
            this.msGraphControlPrecursor.TabIndex = 0;
            this.msGraphControlPrecursor.ContextMenuBuilder += new ZedGraph.ZedGraphControl.ContextMenuBuilderEventHandler(this.GraphControlContextMenuBuilder);
            // 
            // msGraphControlFragment
            // 
            this.msGraphControlFragment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.msGraphControlFragment.EditButtons = System.Windows.Forms.MouseButtons.Left;
            this.msGraphControlFragment.EditModifierKeys = System.Windows.Forms.Keys.None;
            this.msGraphControlFragment.IsEnableVPan = false;
            this.msGraphControlFragment.IsEnableVZoom = false;
            this.msGraphControlFragment.IsShowCopyMessage = false;
            this.msGraphControlFragment.IsZoomOnMouseCenter = true;
            this.msGraphControlFragment.Location = new System.Drawing.Point(0, 0);
            this.msGraphControlFragment.Name = "msGraphControlFragment";
            this.msGraphControlFragment.ScrollGrace = 0D;
            this.msGraphControlFragment.ScrollMaxX = 0D;
            this.msGraphControlFragment.ScrollMaxY = 0D;
            this.msGraphControlFragment.ScrollMaxY2 = 0D;
            this.msGraphControlFragment.ScrollMinX = 0D;
            this.msGraphControlFragment.ScrollMinY = 0D;
            this.msGraphControlFragment.ScrollMinY2 = 0D;
            this.msGraphControlFragment.Size = new System.Drawing.Size(476, 151);
            this.msGraphControlFragment.TabIndex = 0;
            this.msGraphControlFragment.ContextMenuBuilder += new ZedGraph.ZedGraphControl.ContextMenuBuilderEventHandler(this.GraphControlContextMenuBuilder);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.lblMassResolution);
            this.flowLayoutPanel1.Controls.Add(this.tbxMassResolution);
            this.flowLayoutPanel1.Controls.Add(this.cbxTrackSelection);
            this.flowLayoutPanel1.Controls.Add(this.groupBox1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 216);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(784, 51);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // lblMassResolution
            // 
            this.lblMassResolution.AutoSize = true;
            this.lblMassResolution.Location = new System.Drawing.Point(3, 0);
            this.lblMassResolution.Name = "lblMassResolution";
            this.lblMassResolution.Padding = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.lblMassResolution.Size = new System.Drawing.Size(89, 23);
            this.lblMassResolution.TabIndex = 0;
            this.lblMassResolution.Text = "Mass resolution:";
            // 
            // tbxMassResolution
            // 
            this.tbxMassResolution.Location = new System.Drawing.Point(10, 26);
            this.tbxMassResolution.Name = "tbxMassResolution";
            this.tbxMassResolution.Size = new System.Drawing.Size(100, 20);
            this.tbxMassResolution.TabIndex = 1;
            // 
            // cbxTrackSelection
            // 
            this.cbxTrackSelection.AutoSize = true;
            this.cbxTrackSelection.Checked = true;
            this.cbxTrackSelection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxTrackSelection.Location = new System.Drawing.Point(384, 19);
            this.cbxTrackSelection.Name = "cbxTrackSelection";
            this.cbxTrackSelection.Padding = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.cbxTrackSelection.Size = new System.Drawing.Size(105, 27);
            this.cbxTrackSelection.TabIndex = 3;
            this.cbxTrackSelection.Text = "Track selection";
            this.cbxTrackSelection.UseVisualStyleBackColor = true;
            // 
            // lblPrecursorMassShift
            // 
            this.lblPrecursorMassShift.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPrecursorMassShift.AutoSize = true;
            this.lblPrecursorMassShift.Location = new System.Drawing.Point(668, 55);
            this.lblPrecursorMassShift.Name = "lblPrecursorMassShift";
            this.lblPrecursorMassShift.Size = new System.Drawing.Size(107, 13);
            this.lblPrecursorMassShift.TabIndex = 6;
            this.lblPrecursorMassShift.Text = "Extra Precursor Mass";
            // 
            // tbxPrecursorMassShift
            // 
            this.tbxPrecursorMassShift.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxPrecursorMassShift.Location = new System.Drawing.Point(671, 71);
            this.tbxPrecursorMassShift.Name = "tbxPrecursorMassShift";
            this.tbxPrecursorMassShift.Size = new System.Drawing.Size(100, 20);
            this.tbxPrecursorMassShift.TabIndex = 7;
            // 
            // lblFragmentMassShift
            // 
            this.lblFragmentMassShift.AutoSize = true;
            this.lblFragmentMassShift.Location = new System.Drawing.Point(397, 16);
            this.lblFragmentMassShift.Name = "lblFragmentMassShift";
            this.lblFragmentMassShift.Size = new System.Drawing.Size(106, 13);
            this.lblFragmentMassShift.TabIndex = 10;
            this.lblFragmentMassShift.Text = "Extra Fragment Mass";
            // 
            // tbxFragmentMassShift
            // 
            this.tbxFragmentMassShift.Location = new System.Drawing.Point(397, 32);
            this.tbxFragmentMassShift.Name = "tbxFragmentMassShift";
            this.tbxFragmentMassShift.Size = new System.Drawing.Size(120, 20);
            this.tbxFragmentMassShift.TabIndex = 11;
            // 
            // IsotopeDistForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.groupBoxFragment);
            this.Controls.Add(this.groupBoxPrecursor);
            this.Name = "IsotopeDistForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.TabText = "Isotope Distributions";
            this.Text = "Isotope Distributions";
            this.groupBoxFragment.ResumeLayout(false);
            this.groupBoxFragment.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxPrecursor.ResumeLayout(false);
            this.groupBoxPrecursor.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMassIntensity)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.splitContainerGraph.Panel1.ResumeLayout(false);
            this.splitContainerGraph.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGraph)).EndInit();
            this.splitContainerGraph.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBoxLosses;
        private System.Windows.Forms.TextBox tbxPeptideSequence;
        private System.Windows.Forms.Label lblPeptideSequence;
        private System.Windows.Forms.TextBox tbxFragmentCharge;
        private System.Windows.Forms.Label lblCharge;
        private System.Windows.Forms.Label lblPrecursorFormula;
        private System.Windows.Forms.TextBox tbxChemicalFormula;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dataGridViewMassIntensity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMass;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIntensity;
        private System.Windows.Forms.TextBox tbxFragmentIonOrdinal;
        private System.Windows.Forms.Label lblFragmentIonIndex;
        private System.Windows.Forms.Label lblFragmentIonType;
        private System.Windows.Forms.ComboBox comboFragmentIonType;
        private System.Windows.Forms.Label lblPrecursorCharge;
        private System.Windows.Forms.GroupBox groupBoxFragment;
        private System.Windows.Forms.Label lblLosses;
        private System.Windows.Forms.Label lblFragmentFormula;
        private System.Windows.Forms.TextBox tbxFragmentFormula;
        private System.Windows.Forms.GroupBox groupBoxPrecursor;
        private System.Windows.Forms.TextBox tbxIsolationUpper;
        private System.Windows.Forms.Label lblHyphen;
        private System.Windows.Forms.TextBox tbxIsolationLower;
        private System.Windows.Forms.TextBox tbxPrecursorCharge;
        private System.Windows.Forms.SplitContainer splitContainerGraph;
        private MSGraph.MSGraphControl msGraphControlPrecursor;
        private MSGraph.MSGraphControl msGraphControlFragment;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel flowLayoutPanel1;
        private System.Windows.Forms.Label lblMassResolution;
        private System.Windows.Forms.TextBox tbxMassResolution;
        private System.Windows.Forms.CheckBox cbxTrackSelection;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblAverageMass;
        private System.Windows.Forms.TextBox tbxAverageMass;
        private System.Windows.Forms.Label lblMonoisotopicMass;
        private System.Windows.Forms.TextBox tbxMonoMass;
        private System.Windows.Forms.TextBox tbxFragmentMassShift;
        private System.Windows.Forms.Label lblFragmentMassShift;
        private System.Windows.Forms.TextBox tbxPrecursorMassShift;
        private System.Windows.Forms.Label lblPrecursorMassShift;
    }
}