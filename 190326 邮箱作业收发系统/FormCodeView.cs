﻿using CppRunningHelper;
using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EmailHomeworkSystem {
    public partial class FormCodeView : Form {
        private FileInfo fileinfo;

        public FormCodeView() {
            InitializeComponent();
            InitializeUI();
        }

        //----------------------------初始化操作----------------------------

        private void InitializeUI() {
            codeEditor.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("C++.NET");
            codeEditor.Encoding = Encoding.Default;
            codeEditor.Document.FoldingManager.FoldingStrategy = new CppFolding();
        }

        //----------------------------功能操作----------------------------

        public void OpenFile(string filePath) {
            this.Text = filePath;
            this.fileinfo = new FileInfo(filePath);
            codeEditor.Text = File.ReadAllText(filePath, Encoding.Default);
            //textEditor.Text = FormatCode(textEditor.Text); //格式化代码
        }

        //----------------------------界面事件----------------------------

        /// <summary>
        /// 编译运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, EventArgs e) {
            if (!CppHelper.Compile(fileinfo.Directory.FullName)) {
                MessageBox.Show("编译失败！", "Warning");
            }
            if (!CppHelper.Run(fileinfo.Directory.FullName)) {
                MessageBox.Show("运行失败！", "Warning");
            }
            //if (!CppHelper.Clean(fileinfo.Directory.FullName)) {
            //    MessageBox.Show("编译文件清理失败！", "Warning");
            //}
        }
        private void btnSave_MouseMove(object sender, MouseEventArgs e) {
            btnSave.BackColor = Color.LightBlue;
        }
        private void btnSave_MouseLeave(object sender, EventArgs e) {
            btnSave.BackColor = Color.Transparent;
        }
        private void btnSave_MouseDown(object sender, MouseEventArgs e) {
            btnSave.BackColor = Color.SkyBlue;
        }

        private void btnRun_MouseMove(object sender, MouseEventArgs e) {
            btnRun.BackColor = Color.LightBlue;
        }
        private void btnRun_MouseLeave(object sender, EventArgs e) {
            btnRun.BackColor = Color.Transparent;
        }
        private void btnRun_MouseDown(object sender, MouseEventArgs e) {
            btnRun.BackColor = Color.SkyBlue;
        }

        private void textEditor_TextChanged(object sender, System.EventArgs e) {
            codeEditor.Document.FoldingManager.UpdateFoldings(null, null);
            //if (textEditor.Text.Length <= oldJScodeLength) return;
            //var Line = this.textEditor.ActiveTextAreaControl.Caret.Line;
            //var offset = this.textEditor.ActiveTextAreaControl.Caret.Offset;
            //if (offset == 0) return;
            //var LineSegment = textEditor.ActiveTextAreaControl.TextArea.Document.GetLineSegment(Line);
            //if (offset == LineSegment.Length) return;
            //try {
            //    var charT = textEditor.ActiveTextAreaControl.TextArea.Document.GetText(offset, 1);
            //    var charE = "";
            //    switch (charT) {
            //        case "{":
            //            charE = "}";
            //            break;
            //        case "(":
            //            charE = ")";
            //            break;
            //        case "[":
            //            charE = "]";
            //            break;
            //    }
            //    if (!string.IsNullOrEmpty(charE)) {
            //        textEditor.ActiveTextAreaControl.SelectionManager.RemoveSelectedText();
            //        textEditor.ActiveTextAreaControl.Caret.Column = textEditor.ActiveTextAreaControl.Caret.Column + 1;
            //        textEditor.ActiveTextAreaControl.TextArea.InsertChar(charE[0]);
            //        textEditor.ActiveTextAreaControl.Caret.Column = textEditor.ActiveTextAreaControl.Caret.Column - 2;
            //        this.textEditor.ActiveTextAreaControl.TextArea.ScrollToCaret();
            //    }
            //} catch (Exception) {


            //}
        }

        //----------------------------格式化代码----------------------------
        /// <summary>
        /// 格式化代码
        /// </summary>
        /// <param name="code">原来的代码</param>
        /// <returns>格式化好的代码</returns>
        public static string FormatCode(string code) {
            //去除空白行
            //code = RemoveEmptyLines(code);
            StringBuilder sb = new StringBuilder();
            int count = 4; //缩进空格数
            int times = 0;
            string[] lines = code.Split(Environment.NewLine.ToCharArray());
            foreach (var line in lines) {
                if (line.TrimStart().StartsWith("{") || line.TrimEnd().EndsWith("{")) {
                    sb.AppendLine(Indent(count * times) + line.Trim());
                    times++;
                } else if (line.TrimStart().StartsWith("}")) {
                    times--;
                    if (times <= 0) {
                        times = 0;
                    }
                    sb.AppendLine(Indent(count * times) + line.Trim());
                } else {
                    sb.AppendLine(Indent(count * times) + line.Trim());
                }
            }
            return sb.ToString();
        }
        private static string Indent(int spaceNum) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < spaceNum; i++)
                sb.Append(" ");
            return sb.ToString();
        }
    }

    /// <summary>
    /// The class to generate the foldings, it implements ICSharpCode.TextEditor.Document.IFoldingStrategy
    /// </summary>
    public class CppFolding : IFoldingStrategy {
        /// <summary>
        /// Generates the foldings for our document.
        /// </summary>
        /// <param name="document">The current document.</param>
        /// <param name="fileName">The filename of the document.</param>
        /// <param name="parseInformation">Extra parse information, not used in this sample.</param>
        /// <returns>A list of FoldMarkers.</returns>
        public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation) {
            List<FoldMarker> list = new List<FoldMarker>();
            //stack 先进先出
            var startLines = new Stack<int>();
            // Create foldmarkers for the whole document, enumerate through every line.
            for (int i = 0; i < document.TotalNumberOfLines; i++) {
                // Get the text of current line.
                string text = document.GetText(document.GetLineSegment(i));

                if (text.Trim().StartsWith("#region")) { // Look for method starts
                    startLines.Push(i);
                }
                if (text.Trim().StartsWith("#endregion")) { // Look for method endings
                    int start = startLines.Pop();
                    // Add a new FoldMarker to the list.
                    // document = the current document
                    // start = the start line for the FoldMarker
                    // document.GetLineSegment(start).Length = the ending of the current line = the start column of our foldmarker.
                    // i = The current line = end line of the FoldMarker.
                    // 7 = The end column
                    list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, 57, FoldType.Region, "..."));
                }
                //支持嵌套 {}
                if (text.Trim().StartsWith("{") || text.Trim().EndsWith("{")) { // Look for method starts
                    startLines.Push(i);
                }
                if (text.Trim().StartsWith("}") || text.Trim().EndsWith("}")) { // Look for method endings
                    if (startLines.Count > 0) {
                        int start = startLines.Pop();
                        list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, 57, FoldType.TypeBody, "...}"));
                    }
                }
                // /// <summary>
                if (text.Trim().StartsWith("/// <summary>")) { // Look for method starts
                    startLines.Push(i);
                }
                if (text.Trim().StartsWith("/// <returns>")) { // Look for method endings
                    int start = startLines.Pop();
                    string display = document.GetText(document.GetLineSegment(start + 1).Offset, document.GetLineSegment(start + 1).Length);
                    //remove ///
                    display = display.Trim().TrimStart('/');
                    list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, 57, FoldType.TypeBody, display));
                }
            }
            return list;
        }
    }
}
