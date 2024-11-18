// seeds/001_customers_seed.js
exports.seed = function(knex) {
  return knex('CUSTOMERS').del()
    .then(function() {
      return knex('CUSTOMERS').insert([
        { CUSTOMER_ID: 1, CUSTOMER_NAME: 'John Doe', PHONE_NUMBER: '1234567890', EMAIL: 'john.doe@example.com', POINTS: 100, REWARDS: 'Free Coffee' },
        { CUSTOMER_ID: 2, CUSTOMER_NAME: 'Jane Smith', PHONE_NUMBER: '0987654321', EMAIL: 'jane.smith@example.com', POINTS: 200, REWARDS: 'Discount' },
        { CUSTOMER_ID: 3, CUSTOMER_NAME: 'Alice Johnson', PHONE_NUMBER: '1112223333', EMAIL: 'alice.johnson@example.com', POINTS: 150, REWARDS: 'Free Pastry' },
        { CUSTOMER_ID: 4, CUSTOMER_NAME: 'Bob Brown', PHONE_NUMBER: '4445556666', EMAIL: 'bob.brown@example.com', POINTS: 50, REWARDS: 'None' },
        { CUSTOMER_ID: 5, CUSTOMER_NAME: 'Charlie Davis', PHONE_NUMBER: '7778889999', EMAIL: 'charlie.davis@example.com', POINTS: 300, REWARDS: 'Gift Card' },
        { CUSTOMER_ID: 6, CUSTOMER_NAME: 'David Evans', PHONE_NUMBER: '1231231234', EMAIL: 'david.evans@example.com', POINTS: 120, REWARDS: 'Free Coffee' },
        { CUSTOMER_ID: 7, CUSTOMER_NAME: 'Eve Foster', PHONE_NUMBER: '3213214321', EMAIL: 'eve.foster@example.com', POINTS: 80, REWARDS: 'Discount' },
        { CUSTOMER_ID: 8, CUSTOMER_NAME: 'Frank Green', PHONE_NUMBER: '5556667777', EMAIL: 'frank.green@example.com', POINTS: 90, REWARDS: 'Free Pastry' },
        { CUSTOMER_ID: 9, CUSTOMER_NAME: 'Grace Harris', PHONE_NUMBER: '8889990000', EMAIL: 'grace.harris@example.com', POINTS: 110, REWARDS: 'None' },
        { CUSTOMER_ID: 10, CUSTOMER_NAME: 'Hank Irving', PHONE_NUMBER: '9990001111', EMAIL: 'hank.irving@example.com', POINTS: 130, REWARDS: 'Gift Card' },
        { CUSTOMER_ID: 11, CUSTOMER_NAME: 'Ivy Johnson', PHONE_NUMBER: '2223334444', EMAIL: 'ivy.johnson@example.com', POINTS: 140, REWARDS: 'Free Coffee' },
        { CUSTOMER_ID: 12, CUSTOMER_NAME: 'Jack King', PHONE_NUMBER: '3334445555', EMAIL: 'jack.king@example.com', POINTS: 160, REWARDS: 'Discount' },
        { CUSTOMER_ID: 13, CUSTOMER_NAME: 'Karen Lee', PHONE_NUMBER: '4445556666', EMAIL: 'karen.lee@example.com', POINTS: 170, REWARDS: 'Free Pastry' },
        { CUSTOMER_ID: 14, CUSTOMER_NAME: 'Larry Moore', PHONE_NUMBER: '5556667777', EMAIL: 'larry.moore@example.com', POINTS: 180, REWARDS: 'None' },
        { CUSTOMER_ID: 15, CUSTOMER_NAME: 'Mona Nelson', PHONE_NUMBER: '6667778888', EMAIL: 'mona.nelson@example.com', POINTS: 190, REWARDS: 'Gift Card' }
      ]);
    });
};