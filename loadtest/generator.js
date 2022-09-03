const crypto = require('crypto');

const CUSTOMER_IDENTIFIER = [
  "James", "Ruben", "Mark", "Jon", "Matt", "Sue", "Mark", "Harry", "Emma", "Jess", "Tom", "Charlotte", "Paul"
];

module.exports = {
  generateCustomer: function (context, events, done) {
    const customer = CUSTOMER_IDENTIFIER[Math.floor(Math.random() * CUSTOMER_IDENTIFIER.length)];

    context.vars.Id = crypto.randomUUID();
    context.vars.Customer = customer;

    return done();
  },
};