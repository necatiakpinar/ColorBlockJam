using System;
using NUnit.Framework;
using RollicCase.Game.UI;
using RollicCase.Tests.EditMode.Helpers;

namespace RollicCase.Tests.EditMode
{
    public sealed class PopupServiceTests
    {
        PopupService _service;

        [SetUp]
        public void SetUp() => _service = new PopupService();

        [Test]
        public void Open_ShouldShowPopup_WhenNothingIsOpen()
        {
            var popup = new RecordingPopup();

            _service.Open(popup);

            Assert.That(popup.ShowCount, Is.EqualTo(1));
            Assert.That(_service.IsOpen, Is.True);
        }

        [Test]
        public void Open_ShouldThrowAndKeepFirstPopup_WhenAnotherIsOpen()
        {
            var first = new RecordingPopup();
            var second = new RecordingPopup();
            _service.Open(first);

            Assert.Throws<InvalidOperationException>(() => _service.Open(second));

            Assert.That(second.ShowCount, Is.Zero);
            Assert.That(first.HideCount, Is.Zero);
        }

        [Test]
        public void Close_ShouldHidePopupAndAllowTheNext_WhenOpen()
        {
            var first = new RecordingPopup();
            var second = new RecordingPopup();
            _service.Open(first);

            _service.Close();
            _service.Open(second);

            Assert.That(first.HideCount, Is.EqualTo(1));
            Assert.That(second.ShowCount, Is.EqualTo(1));
        }

        [Test]
        public void Close_ShouldThrow_WhenNothingIsOpen()
        {
            Assert.Throws<InvalidOperationException>(() => _service.Close());
        }
    }
}
