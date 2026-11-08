// Security-focused ESLint configuration
// Note: Run 'npm install eslint-plugin-security --save-dev' to install the security plugin

export default [
  {
    ignores: ['dist/**', 'node_modules/**', 'eslint.config.mjs'],
  },
  {
    // Basic security rules without plugin (fallback)
    rules: {
      // Prevent potential security vulnerabilities
      'no-eval': 'error',
      'no-implied-eval': 'error',
      'no-new-func': 'error',
      'no-script-url': 'error',

      // Prevent prototype pollution
      'no-proto': 'error',
      'no-prototype-builtins': 'error',

      // Safe regex practices
      'no-div-regex': 'error',
      'no-regex-spaces': 'error',

      // Prevent dangerous globals
      'no-global-assign': 'error',
      'no-implicit-globals': 'error',

      // Type safety (security-related)
      'valid-typeof': 'error',
      'no-undef': 'error',

      // Prevent XSS in templates
      'no-multi-str': 'error',

      // Console/debug statement removal
      'no-console': 'warn',
      'no-debugger': 'error',
      'no-alert': 'error',
    },
  },
];

// When eslint-plugin-security is installed, uncomment below:
/*
import securityPlugin from 'eslint-plugin-security';

export default [
  {
    ignores: ['dist/**', 'node_modules/**', 'eslint.config.mjs'],
  },
  {
    plugins: {
      security: securityPlugin,
    },
    rules: {
      ...securityPlugin.configs.recommended.rules,
      'security/detect-object-injection': 'error',
      'security/detect-non-literal-regexp': 'error',
      'security/detect-unsafe-regex': 'error',
      'security/detect-buffer-noassert': 'error',
      'security/detect-child-process': 'error',
      'security/detect-disable-mustache-escape': 'error',
      'security/detect-eval-with-expression': 'error',
      'security/detect-no-csrf-before-method-override': 'error',
      'security/detect-non-literal-fs-filename': 'warn',
      'security/detect-non-literal-require': 'warn',
      'security/detect-possible-timing-attacks': 'warn',
      'security/detect-pseudoRandomBytes': 'error',
    },
  },
];
*/
