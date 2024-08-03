---
title: NetVips documentation
documentType: index
_enableSearch: false
_disableNavbar: true
_disableBreadcrumb: true
_disableToc: true
_disableAffix: true
_disableFooter: true
---
<div class="hero">
  <div class="wrap">
    <div class="text">
      <strong>NetVips</strong>
    </div>
    <div class="buttons-unit-small">
      <a class="changelog-link" href="../CHANGELOG.md">Changelog</a><span>|</span><a class="github-link" href="https://github.com/kleisauke/net-vips">View on Github</a>
    </div>
    <div class="minitext">
    .NET binding for the <a href="https://www.libvips.org/">libvips image processing library</a>.
    </div>
    <div class="buttons-unit">
      <a href="introduction.md"><i class="glyphicon glyphicon-send"></i>Introduction</a>
      <a href="xref:NetVips"><i class="glyphicon glyphicon-book"></i>API Documentation</a>
      <a href="https://github.com/kleisauke/net-vips/releases"><i class="glyphicon glyphicon-download"></i>Download Latest</a>
    </div>
  </div>
</div>
<div class="key-section">
  <div class="container">
    <div class="row">
      <div class="col-md-8 col-md-offset-2 text-center">
        <i class="glyphicon glyphicon-dashboard"></i>
        <section>
          <h2>Runs quickly and uses little memory</h2>
          <p class="lead">NetVips is fast and needs little memory. The <a href="https://github.com/kleisauke/net-vips/tree/master/tests/NetVips.Benchmarks">NetVips.Benchmarks</a> project tests NetVips against Magick.NET and ImageSharp.
          NetVips is around 20 times faster than Magick.NET and 3 times faster than ImageSharp.</p>
        </section>
      </div>
    </div>
  </div>
</div>
<div class="counter-key-section">
  <div class="container">
    <div class="row">
      <div class="col-md-8 col-md-offset-2 text-center">
        <i class="glyphicon glyphicon-wrench"></i>
        <section>
          <h2>Around 300 operations supported</h2>
          <p class="lead">It has around <a href="xref:NetVips.Image">300 operations</a> covering arithmetic, histograms, convolution, morphological operations, frequency filtering, colour, resampling, statistics and others.
          It supports a large range of <a href="xref:NetVips.Enums.BandFormat">numeric formats</a>, from 8-bit int to 128-bit complex. Images can have any number of bands.</p>
        </section>
      </div>
    </div>
  </div>
</div>
<div class="key-section">
  <div class="container">
    <div class="row">
      <div class="col-md-8 col-md-offset-2 text-center">
        <i class="glyphicon glyphicon-tags"></i>
        <section>
          <h2>Many image formats supported</h2>
          <p class="lead">It supports a good range of image formats, including JPEG, TIFF, OME-TIFF, PNG, WebP, FITS, Matlab, OpenEXR, PDF, SVG, HDR, PPM, CSV, GIF, Analyze, DeepZoom, and OpenSlide.
          It can also load images via ImageMagick or GraphicsMagick, letting it load formats like DICOM.</p>
        </section>
      </div>
    </div>
  </div>
</div>