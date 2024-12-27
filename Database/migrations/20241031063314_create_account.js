// migrations/xxxx_create_account.js
exports.up = function(knex) {
    return knex.schema.createTable('ACCOUNT', (table) => {
      table.increments('EMPLOYEE_ID');
      table.string('EMP_NAME', 40).notNullable();
      table.string('EMP_ROLE', 40);
      table.integer('ACCESS_LEVEL');
      table.string('USERNAME', 40).unique();
      table.string('USER_PASSWORD', 1000);
    });
  };
  
  exports.down = function(knex) {
    return knex.schema.dropTableIfExists('ACCOUNT');
  };
  