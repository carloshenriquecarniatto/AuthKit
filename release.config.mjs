/**
 * Semantic Release configuration.
 *
 * One lockstep version for every artifact. The version is written to
 * src/dotnet/version.props and to the Angular package manifest by scripts/set-version.mjs,
 * then committed back so local builds and tagged sources agree.
 */
export default {
  branches: ['main'],
  tagFormat: 'v${version}',
  plugins: [
    '@semantic-release/commit-analyzer',
    '@semantic-release/release-notes-generator',
    ['@semantic-release/changelog', { changelogFile: 'CHANGELOG.md' }],
    [
      '@semantic-release/exec',
      {
        prepareCmd: 'node scripts/set-version.mjs ${nextRelease.version}',
        publishCmd: 'bash scripts/publish.sh ${nextRelease.version}',
      },
    ],
    [
      '@semantic-release/git',
      {
        assets: [
          'CHANGELOG.md',
          'src/dotnet/version.props',
          'src/angular/authkit-workspace/projects/authkit/package.json',
          'package.json',
        ],
        message: 'chore(release): ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}',
      },
    ],
    '@semantic-release/github',
  ],
};
