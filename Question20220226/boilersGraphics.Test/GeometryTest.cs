using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class GeometryTest
    {
        [Test]
        public void StreamGeometryテスト()
        {
            var geometry = new StreamGeometry();
            using (var g = geometry.Open())
            {
                g.BeginFigure(new Point(0, 0), true, true);
                g.PolyBezierTo(new Point[] { new Point(1, 1), new Point(2, 0), new Point(3, -1) }, true, false);
            }
            geometry.Freeze();
            Assert.That(geometry.ToString(), Is.EqualTo("M0,0C1,1,2,0,3,-1z"));
        }
    }
}
