// ng-packagr doesn't reliably bundle arbitrary non-component SCSS partials, so this copies
// the shared theme file into dist after the library build, and patches the generated
// package.json's "exports" map (ng-packagr only declares "." and "./package.json" by
// default, which blocks any other subpath — including this one — from resolving) so
// consumers can `@use 'ocrweb.frontend.shared/theme.scss'` from node_modules.
//
// Also runs `npm pack` on the built output: consuming this dist folder via a plain `file:`
// dependency makes npm create a symlink, and a symlinked package's imports of @angular/core
// resolve from ITS OWN node_modules (this workspace's), not the consumer's — two separate
// @angular/core instances in memory breaks DI (NG0203 at runtime). Depending on the packed
// .tgz instead makes npm extract a real copy into the consumer's node_modules, so
// @angular/core resolves to the consumer's single copy like any other real dependency.
const { copyFileSync, readFileSync, writeFileSync } = require('node:fs');
const { execSync } = require('node:child_process');
const { join } = require('node:path');

const distDir = join(__dirname, '..', 'dist', 'ocrweb.frontend.shared');
const src = join(__dirname, '..', 'projects', 'ocrweb.frontend.shared', 'src', 'theme.scss');
const dest = join(distDir, 'theme.scss');

copyFileSync(src, dest);
console.log(`Copied ${src} -> ${dest}`);

const packageJsonPath = join(distDir, 'package.json');
const packageJson = JSON.parse(readFileSync(packageJsonPath, 'utf8'));
packageJson.exports['./theme.scss'] = './theme.scss';
writeFileSync(packageJsonPath, JSON.stringify(packageJson, null, 2) + '\n');
console.log(`Patched ${packageJsonPath} exports with ./theme.scss`);

execSync('npm pack', { cwd: distDir, stdio: 'inherit' });
