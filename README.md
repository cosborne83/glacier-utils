# glacier-utils
This project contains a number of utilities for working with Amazon Glacier archives.

Whilst similar utilities are available elsewhere, the `TreeHashTransform` implements an incremental SHA-256 tree-hash calculation, which unlike many publically-available implementations does not require all of the individual chunk hashes to be kept in memory. This may be useful if calculating tree hashes over very large files where the individual chunk hashes are not required.
