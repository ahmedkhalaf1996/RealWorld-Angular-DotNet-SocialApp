import './commands'

beforeEach(() => {
  localStorage.setItem('token', 'mock-jwt-token-for-testing');
  localStorage.setItem('user', JSON.stringify({
    userId : 1,
    username: 'testuser',
    email: 'testuser@example.com'
   }));
});

afterEach(() => {
  localStorage.clear();
});
