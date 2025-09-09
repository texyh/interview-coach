import {
  BarChartOutlined,
  BookOutlined,
  RobotOutlined,
  SettingOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { Button, Card, ConfigProvider, Layout, Space, Typography } from "antd";

const { Header, Content, Footer, Sider } = Layout;
const { Title, Text } = Typography;

function App() {
  return (
    <ConfigProvider
      theme={{
        token: {
          colorPrimary: "#4f46e5", // Indigo
          borderRadius: 8,
        },
      }}
    >
      <Layout className="min-h-screen">
        {/* Sidebar */}
        <Sider theme="light" className="shadow-sm" width={280}>
          <div className="p-6">
            <div className="flex items-center space-x-2 mb-8">
              <RobotOutlined className="text-2xl text-indigo-600" />
              <Title level={4} className="m-0 text-slate-900">
                AIPA
              </Title>
            </div>

            <Space direction="vertical" className="w-full" size="small">
              <Button
                type="text"
                icon={<BookOutlined />}
                className="w-full justify-start h-10"
                size="large"
              >
                Interview Prep
              </Button>
              <Button
                type="text"
                icon={<BarChartOutlined />}
                className="w-full justify-start h-10"
                size="large"
              >
                Analytics
              </Button>
              <Button
                type="text"
                icon={<UserOutlined />}
                className="w-full justify-start h-10"
                size="large"
              >
                Profile
              </Button>
              <Button
                type="text"
                icon={<SettingOutlined />}
                className="w-full justify-start h-10"
                size="large"
              >
                Settings
              </Button>
            </Space>
          </div>
        </Sider>

        {/* Main Layout */}
        <Layout>
          {/* Header */}
          <Header className="bg-white shadow-sm px-6 flex items-center justify-between">
            <Title level={3} className="m-0 text-slate-900">
              AI Interview Preparation Assistant
            </Title>
            <Button type="primary" className="btn-primary">
              Start Interview
            </Button>
          </Header>

          {/* Content */}
          <Content className="p-6">
            <div className="max-w-6xl mx-auto">
              {/* Welcome Section */}
              <Card className="card mb-6">
                <div className="text-center">
                  <RobotOutlined className="text-6xl text-indigo-600 mb-4" />
                  <Title level={2} className="text-slate-900 mb-2">
                    Welcome to AIPA
                  </Title>
                  <Text className="text-slate-600 text-lg">
                    Your AI-powered companion for interview preparation and
                    career success
                  </Text>
                </div>
              </Card>

              {/* Feature Cards */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-6">
                <Card className="card hover:shadow-md transition-shadow duration-200">
                  <div className="text-center">
                    <BookOutlined className="text-3xl text-indigo-600 mb-3" />
                    <Title level={4} className="text-slate-900 mb-2">
                      Smart Preparation
                    </Title>
                    <Text className="text-slate-600">
                      AI-driven interview questions tailored to your industry
                      and role
                    </Text>
                  </div>
                </Card>

                <Card className="card hover:shadow-md transition-shadow duration-200">
                  <div className="text-center">
                    <BarChartOutlined className="text-3xl text-indigo-600 mb-3" />
                    <Title level={4} className="text-slate-900 mb-2">
                      Performance Analytics
                    </Title>
                    <Text className="text-slate-600">
                      Detailed insights and progress tracking for continuous
                      improvement
                    </Text>
                  </div>
                </Card>

                <Card className="card hover:shadow-md transition-shadow duration-200">
                  <div className="text-center">
                    <UserOutlined className="text-3xl text-indigo-600 mb-3" />
                    <Title level={4} className="text-slate-900 mb-2">
                      Personalized Coaching
                    </Title>
                    <Text className="text-slate-600">
                      Resume analysis and customized preparation plans
                    </Text>
                  </div>
                </Card>
              </div>

              {/* Quick Actions */}
              <Card className="card">
                <div className="text-center">
                  <Title level={4} className="text-slate-900 mb-4">
                    Ready to start your interview preparation?
                  </Title>
                  <Space size="large">
                    <Button type="primary" size="large" className="btn-primary">
                      Begin Mock Interview
                    </Button>
                    <Button size="large" className="btn-secondary">
                      Upload Resume
                    </Button>
                    <Button size="large" className="btn-secondary">
                      View Progress
                    </Button>
                  </Space>
                </div>
              </Card>
            </div>
          </Content>

          {/* Footer */}
          <Footer className="bg-white text-center text-slate-600">
            AIPA - AI Interview Preparation Assistant ©2025
          </Footer>
        </Layout>
      </Layout>
    </ConfigProvider>
  );
}

export default App;
