// JSFuck Deobfuscator - WinForms GUI
// Target: .NET Framework 4.5+ / C# 5
// Compile: csc JSFuckDeobfuscator.cs /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:Microsoft.CSharp.dll
// Or open in Visual Studio and add references to System.Windows.Forms, System.Drawing, Microsoft.CSharp

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace JSFuckDeobfuscator
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string filePath = "decoder.js";
            string jsContent = @"const vm = require(""vm""); const fs = require(""fs""); function splitChunks(code) { const chunks = []; let i = 0; const n = code.length; while (i < n) { while (i < n && (code[i] === '+' || /\s/.test(code[i]))) i++; if (i >= n) break; let depth = 0; let start = i; let started = false; while (i < n) { const c = code[i]; if (c === '(' || c === '[') { depth++; started = true; } else if (c === ')' || c === ']') { depth--; } i++; if (started && depth === 0) { let j = i; while (j < n && /\s/.test(code[j])) j++; if (j < n && code[j] === '[') { continue; } break; } } const chunk = code.slice(start, i).trim(); if (chunk) chunks.push(chunk); } return chunks; } function createSandbox() { const sandbox = {}; sandbox.global = sandbox; sandbox.globalThis = sandbox; sandbox.eval = function(code) { return code; }; const safeFunction = (...args) => { const body = String(args[args.length - 1]).trim(); if (body === ""return eval"") { return () => sandbox.eval; } if (body === ""return Function"") { return () => sandbox.Function; } if (body.startsWith(""return "")) { const expr = body.slice(7).trim(); return () => expr; } return function () { return body; }; }; sandbox.Function = safeFunction; sandbox.String = function(v) { return String(v); }; sandbox.String.prototype = String.prototype; const proxyHandler = { get(target, prop) { if (prop === ""constructor"") { return safeFunction; } return target[prop]; } }; sandbox.Array = new Proxy(Array, proxyHandler); sandbox.Object = new Proxy(Object, proxyHandler); sandbox.Boolean = new Proxy(Boolean, proxyHandler); sandbox.Number = new Proxy(Number, proxyHandler); sandbox.RegExp = new Proxy(RegExp, proxyHandler); return vm.createContext(sandbox); } function evalChunk(context, chunk) { const trimmed = chunk.trim(); if (trimmed === ""()"") { return """"; } try { return String(vm.runInContext(chunk, context)); } catch { return null; } } function decodeJSFuck(code) { const chunks = splitChunks(code); if (chunks.length === 0) { return """"; } const context = createSandbox(); const results = chunks.map(chunk => evalChunk(context, chunk)); return results .map(v => v ?? ""?"") .join(""""); } // Main const jsfuckfile = process.argv[2]; if (!jsfuckfile) { process.exit(1); } const jsfuck = fs.readFileSync(jsfuckfile, ""utf8""); let decoded = decodeJSFuck(jsfuck); // Remove everything before eval const evalIndex = decoded.indexOf(""eval""); if (evalIndex !== -1) { decoded = decoded.slice(evalIndex + 4); } // Output ONLY decoded text process.stdout.write(decoded);";
            File.WriteAllText(filePath, jsContent);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        // --- Controls ---
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private SplitContainer splitMain;
        private SplitContainer splitRight;

        private RichTextBox txtInput;
        private RichTextBox txtOutput;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblChunkCount;
        private ToolStripProgressBar progressBar;
        private Panel diagramPanel;
        private ToolStripButton btnDecode;
        private ToolStripButton btnClear;
        private ToolStripButton btnCopy;

        // Colors — dark decompiler theme
        static readonly Color BgDark    = Color.FromArgb(30, 30, 30);
        static readonly Color BgPanel   = Color.FromArgb(37, 37, 38);
        static readonly Color BgHeader  = Color.FromArgb(45, 45, 48);
        static readonly Color Accent    = Color.FromArgb(78, 201, 176);   // teal
        static readonly Color AccentAlt = Color.FromArgb(220, 220, 170);  // yellow
        static readonly Color TextMain  = Color.FromArgb(212, 212, 212);
        static readonly Color TextMuted = Color.FromArgb(110, 110, 110);
        static readonly Color Border    = Color.FromArgb(60, 60, 60);
        static readonly Color SelBg     = Color.FromArgb(38, 79, 120);

        private void DrawDiagram(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            using (Font title = new Font("Segoe UI", 12, FontStyle.Bold))
            using (Font text = new Font("Consolas", 10))
            using (Pen pen = new Pen(Accent, 3))
            using (Brush box = new SolidBrush(BgHeader))
            using (Brush fg = new SolidBrush(TextMain))
            {
                int x = 80;
                int y = 40;
                int w = 220;
                int h = 70;
                int gap = 45;

                string[] steps =
                {
                    "JSFuck Source",
                    "C# GUI",
                    "Temporary .js File",
                    "Node.js decode.js",
                    "Decoded Output"
                };

                for (int i = 0; i < steps.Length; i++)
                {
                    Rectangle r = new Rectangle(x, y + i * (h + gap), w, h);

                    g.FillRectangle(box, r);
                    g.DrawRectangle(pen, r);

                    SizeF size = g.MeasureString(steps[i], text);

                    g.DrawString(
                        steps[i],
                        text,
                        fg,
                        r.X + (w - size.Width) / 2,
                        r.Y + (h - size.Height) / 2
                    );

                    if (i < steps.Length - 1)
                    {
                        int cx = x + w / 2;

                        g.DrawLine(
                            pen,
                            cx,
                            r.Bottom,
                            cx,
                            r.Bottom + gap
                        );

                        g.DrawLine(
                            pen,
                            cx,
                            r.Bottom + gap,
                            cx - 8,
                            r.Bottom + gap - 10
                        );

                        g.DrawLine(
                            pen,
                            cx,
                            r.Bottom + gap,
                            cx + 8,
                            r.Bottom + gap - 10
                        );
                    }
                }
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form
            this.Text = "JSFD";
            this.Size = new Size(1100, 720);
            this.MinimumSize = new Size(800, 550);
            this.BackColor = BgDark;
            this.ForeColor = TextMain;
            this.Font = new Font("Segoe UI", 9f);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;

            // ── Menu ──────────────────────────────────────────────────
            menuStrip = new MenuStrip { BackColor = BgHeader, ForeColor = TextMain, Renderer = new DarkRenderer() };
            var fileMenu = new ToolStripMenuItem("File") { ForeColor = TextMain };
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Open .js file…", null, OnOpenFile) { ForeColor = TextMain });
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit()) { ForeColor = TextMain });

            var editMenu = new ToolStripMenuItem("Edit") { ForeColor = TextMain };
            editMenu.DropDownItems.Add(new ToolStripMenuItem("Clear all", null, OnClear) { ForeColor = TextMain });
            editMenu.DropDownItems.Add(new ToolStripMenuItem("Copy output", null, OnCopy) { ForeColor = TextMain });

            var viewMenu = new ToolStripMenuItem("View") { ForeColor = TextMain };
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu });

            // ── Toolbar ───────────────────────────────────────────────
            toolStrip = new ToolStrip { BackColor = BgHeader, Renderer = new DarkRenderer(), GripStyle = ToolStripGripStyle.Hidden };

            btnDecode = new ToolStripButton("▶  Decode") { ForeColor = Accent, Font = new Font("Segoe UI", 9f, FontStyle.Bold), DisplayStyle = ToolStripItemDisplayStyle.Text };
            btnDecode.Click += OnDecode;

            btnClear = new ToolStripButton("✕  Clear") { ForeColor = TextMain, DisplayStyle = ToolStripItemDisplayStyle.Text };
            btnClear.Click += OnClear;

            btnCopy = new ToolStripButton("⎘  Copy output") { ForeColor = TextMain, DisplayStyle = ToolStripItemDisplayStyle.Text };
            btnCopy.Click += OnCopy;

            progressBar = new ToolStripProgressBar { Width = 120, Visible = false };

            toolStrip.Items.AddRange(new ToolStripItem[] {
                btnDecode,
                new ToolStripSeparator(),
                btnClear,
                new ToolStripSeparator(),
                btnCopy,
                new ToolStripSeparator(),
                progressBar
            });

            // ── Status bar ────────────────────────────────────────────
            statusStrip = new StatusStrip { BackColor = BgHeader, SizingGrip = false };
            lblStatus     = new ToolStripStatusLabel("Ready") { ForeColor = TextMuted, Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            lblChunkCount = new ToolStripStatusLabel("") { ForeColor = Accent };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblChunkCount });

            // ── Main split (left: input | right: chunks+output) ───────
            splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = Border,
            };

            // ── Left panel — input ────────────────────────────────────
            var lblInput = MakeHeader("INPUT  ·  JSFuck source");
            txtInput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = BgPanel,
                ForeColor = AccentAlt,
                Font = new Font("Consolas", 10f),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false,
                AcceptsTab = true,
            };
            txtInput.TextChanged += (s, e) => SetStatus("Ready — press Decode");

            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = BgPanel };
            leftPanel.Controls.Add(txtInput);
            leftPanel.Controls.Add(lblInput);

            splitMain.Panel1.Controls.Add(leftPanel);

            // ── Right split (top: chunk list | bottom: output) ────────
            splitRight = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                BackColor = Border,
            };

            // Chunk list
            // Diagram
            var lblDiagram = MakeHeader("DECODE PIPELINE  ·  Node.js JSFuck decoder");

            diagramPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPanel
            };

            diagramPanel.Paint += DrawDiagram;

            var diagramContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPanel
            };

            diagramContainer.Controls.Add(diagramPanel);
            diagramContainer.Controls.Add(lblDiagram);

            splitRight.Panel1.Controls.Add(diagramContainer);

            var chunkPanel = new Panel { Dock = DockStyle.Fill, BackColor = BgPanel };
            splitRight.Panel1.Controls.Add(chunkPanel);

            // Output box
            var lblOut = MakeHeader("OUTPUT  ·  decoded string");
            txtOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = BgPanel,
                ForeColor = Accent,
                Font = new Font("Consolas", 11f),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                WordWrap = true,
            };
            var outPanel = new Panel { Dock = DockStyle.Fill, BackColor = BgPanel };
            outPanel.Controls.Add(txtOutput);
            outPanel.Controls.Add(lblOut);
            splitRight.Panel2.Controls.Add(outPanel);

            splitMain.Panel2.Controls.Add(splitRight);

            // ── Wire up ───────────────────────────────────────────────
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(splitMain);
            this.Controls.Add(toolStrip);
            this.Controls.Add(menuStrip);
            this.Controls.Add(statusStrip);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // ── Header label factory ──────────────────────────────────────
        private Label MakeHeader(string text)
        {
            return new Label
            {
                Text = "  " + text,
                Dock = DockStyle.Top,
                Height = 24,
                BackColor = BgHeader,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8f),
                TextAlign = ContentAlignment.MiddleLeft,
            };
        }

        // ── Owner-draw ListView ───────────────────────────────────────
        private void DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BgHeader), e.Bounds);
            e.Graphics.DrawString(e.Header.Text, new Font("Segoe UI", 8.5f, FontStyle.Bold),
                new SolidBrush(TextMuted), e.Bounds.X + 4, e.Bounds.Y + 4);
        }

        private void DrawListItem(object sender, DrawListViewItemEventArgs e)
        {
            var bg = e.Item.Selected ? SelBg : (e.ItemIndex % 2 == 0 ? BgPanel : Color.FromArgb(40, 40, 42));
            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
        }

        private void DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var bg = e.Item.Selected ? SelBg : (e.ItemIndex % 2 == 0 ? BgPanel : Color.FromArgb(40, 40, 42));
            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);

            Color fg = TextMain;
            if (e.ColumnIndex == 1) fg = Accent;          // char column
            if (e.ColumnIndex == 2) fg = TextMuted;        // codepoint
            if (e.ColumnIndex == 3) fg = AccentAlt;        // expression

            var rect  = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height);
        }

        // ── Chunk selected → highlight expression in input ────────────

        // ── Decode ────────────────────────────────────────────────────
        private void OnDecode(object sender, EventArgs e)
        {
            string code = txtInput.Text.Trim();

            if (string.IsNullOrEmpty(code))
            {
                SetStatus("Paste JSFuck first");
                return;
            }

            btnDecode.Enabled = false;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            SetStatus("Running Node.js decoder...");

            Thread t = new Thread(() =>
            {
                try
                {
                    string tempFile = Path.Combine(
                        Path.GetTempPath(),
                        "jsfuck_input_" + Guid.NewGuid().ToString() + ".js"
                    );

                    File.WriteAllText(tempFile, code);

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "node",
                        Arguments = "\"decode.js\" \"" + tempFile + "\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Application.StartupPath
                    };

                    using (Process proc = Process.Start(psi))
                    {
                        string output = proc.StandardOutput.ReadToEnd();
                        string error = proc.StandardError.ReadToEnd();

                        proc.WaitForExit();

                        this.Invoke(new Action(() =>
                        {
                            if (!string.IsNullOrEmpty(error))
                            {
                                txtOutput.Text = "Node.js Error:\r\n" + error;
                                SetStatus("Decoder failed");
                            }
                            else
                            {
                                txtOutput.Text = output;
                                SetStatus("Decode complete");
                            }

                            progressBar.Visible = false;
                            btnDecode.Enabled = true;
                        }));
                    }

                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        txtOutput.Text = ex.ToString();
                        progressBar.Visible = false;
                        btnDecode.Enabled = true;
                        SetStatus("Failed to run Node.js");
                    }));
                }
            });

            t.IsBackground = true;
            t.Start();
        }

        // ── Evaluate a single JSFuck expression via Roslyn/CSharpCodeProvider ──
        // We use the Jint-free approach: embed in a tiny script and run it
        // via a CSharpCodeProvider wrapper that calls a JS engine.
        // Since .NET Framework doesn't ship V8, we evaluate using the
        // Microsoft.JScript engine available in .NET 4.x.

        // Very basic static patterns as absolute last resort

        // ── File open ─────────────────────────────────────────────────
        private void OnOpenFile(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "JavaScript files|*.js|Text files|*.txt|All files|*.*" })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtInput.Text = System.IO.File.ReadAllText(dlg.FileName);
                    SetStatus("Loaded: " + dlg.FileName);
                }
            }
        }

        // ── Clear / Copy ──────────────────────────────────────────────
        private void OnClear(object sender, EventArgs e)
        {
            txtInput.Clear();
            txtOutput.Clear();
            lblChunkCount.Text = "";
            SetStatus("Ready");
        }

        private void OnCopy(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtOutput.Text))
            {
                Clipboard.SetText(txtOutput.Text);
                SetStatus("Output copied to clipboard");
            }
        }

        private void SetStatus(string msg)
        {
            lblStatus.Text = msg;
        }
    }

    // ── Dark menu/toolbar renderer ────────────────────────────────────
    class DarkRenderer : ToolStripProfessionalRenderer
    {
        static readonly Color Bg     = Color.FromArgb(45, 45, 48);
        static readonly Color Hover  = Color.FromArgb(62, 62, 66);
        static readonly Color Border = Color.FromArgb(60, 60, 60);

        public DarkRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (var p = new Pen(Border))
                e.Graphics.DrawLine(p, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1);
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var btn = e.Item as ToolStripButton;
            if (btn != null && (btn.Pressed || btn.Selected))
            {
                e.Graphics.FillRectangle(new SolidBrush(Hover), new Rectangle(Point.Empty, e.Item.Size));
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var rect = new Rectangle(Point.Empty, e.Item.Size);
            if (e.Item.Selected)
                e.Graphics.FillRectangle(new SolidBrush(Hover), rect);
            else
                e.Graphics.FillRectangle(new SolidBrush(Bg), rect);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using (var p = new Pen(Border))
            {
                if (e.Vertical)
                    e.Graphics.DrawLine(p, e.Item.Width / 2, 2, e.Item.Width / 2, e.Item.Height - 2);
                else
                    e.Graphics.DrawLine(p, 4, e.Item.Height / 2, e.Item.Width - 4, e.Item.Height / 2);
            }
        }
    }

    class DarkColorTable : ProfessionalColorTable
    {
        static readonly Color Bg     = Color.FromArgb(45, 45, 48);
        static readonly Color Border = Color.FromArgb(60, 60, 60);

        public override Color MenuStripGradientBegin { get { return Bg; } }
        public override Color MenuStripGradientEnd { get { return Bg; } }
        public override Color ToolStripGradientBegin { get { return Bg; } }
        public override Color ToolStripGradientMiddle { get { return Bg; } }
        public override Color ToolStripGradientEnd { get { return Bg; } }
        public override Color ToolStripBorder { get { return Border; } }
        public override Color MenuBorder { get { return Border; } }
        public override Color MenuItemBorder { get { return Border; } }
        public override Color MenuItemSelected { get { return Color.FromArgb(62, 62, 66); } }
        public override Color MenuItemSelectedGradientBegin { get { return Color.FromArgb(62, 62, 66); } }
        public override Color MenuItemSelectedGradientEnd { get { return Color.FromArgb(62, 62, 66); } }
        public override Color ImageMarginGradientBegin { get { return Bg; } }
        public override Color ImageMarginGradientMiddle { get { return Bg; } }
        public override Color ImageMarginGradientEnd { get { return Bg; } }
        public override Color ToolStripDropDownBackground { get { return Bg; } }
    }
}