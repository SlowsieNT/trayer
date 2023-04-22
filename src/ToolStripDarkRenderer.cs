using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

namespace trayer
{
    public partial class ToolStripDarkRenderer : ToolStripProfessionalRenderer
    {

        Color m_ColorMenuArrow = Color.FromArgb(240, 240, 240);
        Color m_ColorCheckSquare = Color.FromArgb(0, 192, 0);
        Color m_ColorCheckMark = Color.FromArgb(240, 240, 240);
        Color m_ColorMenuItemText = Color.FromArgb(240, 240, 240);

        public ToolStripDarkRenderer() : base(new DarkMenuStripColorTable()) { }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
            if (e != null)
                e.ArrowColor = m_ColorMenuArrow;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
            if (e != null) {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rectImage = new Rectangle(e.ImageRectangle.Location, e.ImageRectangle.Size);
                rectImage.Inflate(-1, -1);
                using (var p = new Pen(m_ColorCheckSquare, 1)) {
                    g.DrawRectangle(p, rectImage);
                }
                var rectCheck = rectImage;
                rectCheck.Width = rectImage.Width - 8;
                rectCheck.Height = rectImage.Height - 8;
                rectCheck.X += 4;
                rectCheck.Y += 4;
                using (var p = new Pen(m_ColorCheckMark, 1)) {
                    g.DrawLines(p, new[] { new Point(rectCheck.Left, rectCheck.Bottom - rectCheck.Height / 2), new Point(rectCheck.Left + rectCheck.Width / 3, rectCheck.Bottom), new Point(rectCheck.Right, rectCheck.Top) });
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            if (e != null) {
                var textRect = e.TextRectangle;
                textRect.Height = e.Item.Height - 4;
                e.TextRectangle = textRect;
                e.TextFormat |= TextFormatFlags.VerticalCenter;
                e.TextColor = m_ColorMenuItemText;
            }
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            if (e == null || !e.Item.Enabled)
                return;
            base.OnRenderMenuItemBackground(e);
        }
    }
    public class DarkMenuStripColorTable : ProfessionalColorTable {
        Color m_ColorMenuBorder = Color.FromArgb(64, 64, 64);
        Color m_ColorMenuItemSelected = Color.FromArgb(96, 96, 96);
        Color m_ColorBackground = Color.FromArgb(48, 48, 48);
        Color m_ColorSeparator = Color.FromArgb(64, 64, 64);
        Color m_ColorStatusStripGradient = Color.FromArgb(240, 240, 240);
        Color m_ColorButtonSelected = Color.FromArgb(48, 48, 48);
        Color m_ColorButtonPressed = Color.FromArgb(48, 48, 48);

        public override Color ToolStripDropDownBackground {
            get {
                return m_ColorBackground;
            }
        }

        public override Color MenuStripGradientBegin {
            get {
                return m_ColorBackground;
            }
        }

        public override Color MenuStripGradientEnd {
            get {
                return m_ColorBackground;
            }
        }

        public override Color CheckBackground {
            get {
                return m_ColorBackground;
            }
        }

        public override Color CheckPressedBackground {
            get {
                return m_ColorBackground;
            }
        }

        public override Color CheckSelectedBackground {
            get {
                return m_ColorBackground;
            }
        }

        public override Color MenuItemSelected {
            get {
                return m_ColorMenuItemSelected;
            }
        }

        public override Color ImageMarginGradientBegin {
            get {
                return m_ColorBackground;
            }
        }

        public override Color ImageMarginGradientMiddle {
            get {
                return m_ColorBackground;
            }
        }

        public override Color ImageMarginGradientEnd {
            get {
                return m_ColorBackground;
            }
        }

        public override Color MenuItemBorder {
            get {
                return m_ColorMenuItemSelected;
            }
        }

        public override Color MenuBorder {
            get {
                return m_ColorMenuBorder;
            }
        }

        public override Color SeparatorDark {
            get {
                return m_ColorSeparator;
            }
        }

        public override Color SeparatorLight {
            get {
                return m_ColorSeparator;
            }
        }

        public override Color StatusStripGradientBegin {
            get {
                return m_ColorStatusStripGradient;
            }
        }

        public override Color StatusStripGradientEnd {
            get {
                return m_ColorStatusStripGradient;
            }
        }

        public override Color ButtonSelectedGradientBegin {
            get {
                return m_ColorButtonSelected;
            }
        }

        public override Color ButtonSelectedGradientMiddle {
            get {
                return m_ColorButtonSelected;
            }
        }

        public override Color ButtonSelectedGradientEnd {
            get {
                return m_ColorButtonSelected;
            }
        }

        public override Color ButtonSelectedBorder {
            get {
                return m_ColorButtonSelected;
            }
        }

        public override Color ButtonPressedGradientBegin {
            get {
                return m_ColorButtonPressed;
            }
        }

        public override Color ButtonPressedGradientMiddle {
            get {
                return m_ColorButtonPressed;
            }
        }

        public override Color ButtonPressedGradientEnd {
            get {
                return m_ColorButtonPressed;
            }
        }

        public override Color ButtonPressedBorder {
            get {
                return m_ColorButtonPressed;
            }
        }
    }
}