// seeds/002_account_seed.js
exports.seed = function(knex) {
  return knex('ACCOUNT').del()
    .then(function() {
      return knex('ACCOUNT').insert([
        { EMPLOYEE_ID: 1, EMP_NAME: 'Admin User', EMP_ROLE: 'Admin', ACCESS_LEVEL: 1, USERNAME: 'admin', USER_PASSWORD: 'gsP5Um5JsSg5DPk0wXmumKO/nY9oJKHtNqqs85CpGHU=:810cd2d9-bee5-4107-aead-b2d15d0e7a77', EMAIL: 'admin123@gmail.com', SALARY: 3000000 },
        { EMPLOYEE_ID: 2, EMP_NAME: 'Tester 1', EMP_ROLE: 'Staff', ACCESS_LEVEL: 2, USERNAME: 'Tester1', USER_PASSWORD: 'A+ukDEN5YwOoLTst0nrbCdNq4LOhJTVUad2vExY2YQ0=:87fa11bd-f1bd-4b98-a794-0b4dddb4b2aa', EMAIL: 'php2692004@gmail.com', SALARY: 3000000 },
        { EMPLOYEE_ID: 3, EMP_NAME: 'Tester 2', EMP_ROLE: 'Staff', ACCESS_LEVEL: 2, USERNAME: 'Tester2', USER_PASSWORD: 'A+ukDEN5YwOoLTst0nrbCdNq4LOhJTVUad2vExY2YQ0=:87fa11bd-f1bd-4b98-a794-0b4dddb4b2aa', EMAIL: 'contact.quanminhle@gmail.com', SALARY: 3000000 },
        { EMPLOYEE_ID: 4, EMP_NAME: 'Tester 3', EMP_ROLE: 'Staff', ACCESS_LEVEL: 2, USERNAME: 'Tester3', USER_PASSWORD: 'A+ukDEN5YwOoLTst0nrbCdNq4LOhJTVUad2vExY2YQ0=:87fa11bd-f1bd-4b98-a794-0b4dddb4b2aa', EMAIL: 'leeqq3394@gmail.com', SALARY: 3000000  },
        { EMPLOYEE_ID: 5, EMP_NAME: 'Tester 4', EMP_ROLE: 'Staff', ACCESS_LEVEL: 2, USERNAME: 'Tester4', USER_PASSWORD: 'A+ukDEN5YwOoLTst0nrbCdNq4LOhJTVUad2vExY2YQ0=:87fa11bd-f1bd-4b98-a794-0b4dddb4b2aa', EMAIL: 'contact.quanminhle@gmail.com', SALARY: 3000000  },
        { EMPLOYEE_ID: 6, EMP_NAME: 'Nguyen', EMP_ROLE: 'Admin', ACCESS_LEVEL: 1, USERNAME: 'Thienne', USER_PASSWORD: 'BwNrJBiApdz6LsE+Yp0sFd4KrKTLsgzvcixqSJ//4E8=:4ffca253-bcfd-4f6b-8a5c-fbefb58005be',EMAIL: 'minhthienqbatri28@gmail.com', SALARY: 3000000  },
        { EMPLOYEE_ID: 7, EMP_NAME: 'NguyenMinh', EMP_ROLE: 'Staff', ACCESS_LEVEL: 2, USERNAME: 'Thienne2', USER_PASSWORD: 'BwNrJBiApdz6LsE+Yp0sFd4KrKTLsgzvcixqSJ//4E8=:4ffca253-bcfd-4f6b-8a5c-fbefb58005be',EMAIL: 'minhthienqbatri29@gmail.com', SALARY: 3000000  }
      ]);
    });
};