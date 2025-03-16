const ddPlugin = require('dd-trace/esbuild')
const esbuild = require('esbuild')

esbuild.build({
  entryPoints: ['./src/lambda/setBaking.ts'],
  bundle: true,
  minify: true,
  outfile: 'out/setBaking/index.js',
  plugins: [ddPlugin],
  platform: 'node', // allows built-in modules to be required
  target: ['node20'],
  external: [
    // esbuild cannot bundle native modules
    '@datadog/native-metrics',

    // required if you use profiling
    '@datadog/pprof',

    // required if you use Datadog security features
    '@datadog/native-appsec',
    '@datadog/native-iast-taint-tracking',
    '@datadog/native-iast-rewriter',

    // required if you encounter graphql errors during the build step
    'graphql/language/visitor',
    'graphql/language/printer',
    'graphql/utilities',
    '@aws-sdk/client-sqs',
    '@aws-sdk/client-secrets-manager'
  ]
}).catch((err) => {
  console.error(err)
  process.exit(1)
})